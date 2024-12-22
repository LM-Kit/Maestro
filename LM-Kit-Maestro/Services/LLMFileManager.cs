using CommunityToolkit.Mvvm.ComponentModel;
using LMKit.Maestro.Helpers;
using LMKit.Maestro.UI;
using LMKit.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LMKit.Maestro.Services;


/// <summary>
/// This service is intended to be used as a singleton via Dependency Injection. 
/// Please register with <c>services.AddSingleton&lt;LLMFileManager&gt;()</c>.
/// </summary>
public partial class LLMFileManager : ObservableObject, ILLMFileManager
{
#if WINDOWS
    private readonly FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();
#endif

#if DEBUG
    private static int InstanceCount = 0;
#endif

    private readonly FileSystemEntryRecorder _fileSystemEntryRecorder = new FileSystemEntryRecorder();
    private readonly IAppSettingsService _appSettingsService;
    private readonly HttpClient _httpClient;
    private bool _enablePredefinedModels = true; //todo: Implement this as a configurable option in the configuration panel 
    private bool _enableCustomModels = true;  //todo: Implement this as a configurable option in the configuration panel 

    private readonly Dictionary<Uri, FileDownloader> _fileDownloads = new Dictionary<Uri, FileDownloader>();

    private delegate bool ModelDownloadingProgressCallback(string path, long? contentLength, long bytesRead);
    public virtual event NotifyCollectionChangedEventHandler? SortedModelCollectionChanged;

    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _collectModelFilesTask;

    public ReadOnlyObservableCollection<ModelCard> UserModels { get; } //todo: rename to SortedModels
    public ReadOnlyObservableCollection<ModelCard> UnsortedModels { get; }


    private ObservableCollection<ModelCard> _sortedModels { get; } = new ObservableCollection<ModelCard>();
    private ObservableCollection<ModelCard> _unsortedModels { get; } = new ObservableCollection<ModelCard>();

    [ObservableProperty]
    private long _totalModelSize;

    [ObservableProperty]
    private int _downloadedCount;

    [ObservableProperty]
    private bool _fileCollectingInProgress;

    private string _modelStorageDirectory = string.Empty;
    public string ModelStorageDirectory
    {
        get => _modelStorageDirectory;
        set
        {
            if (string.CompareOrdinal(_modelStorageDirectory, value) != 0)
            {
                _modelStorageDirectory = value;
#if WINDOWS
                _fileSystemWatcher.Path = value;
#endif
                OnModelStorageDirectoryChanged();
            }
        }
    }

    public event EventHandler? FileCollectingCompleted;
#if BETA_DOWNLOAD_MODELS
    public event EventHandler? ModelDownloadingProgressed;
    public event EventHandler? ModelDownloadingCompleted;
#endif

    public LLMFileManager(IAppSettingsService appSettingsService, HttpClient httpClient)
    {
#if DEBUG
        if (InstanceCount > 0)
        {
            throw new InvalidProgramException("Invalid operation: Only one instance of this class should be created, and it must be instantiated through dependency injection.");
        }
        InstanceCount++;
#endif

        UserModels = new ReadOnlyObservableCollection<ModelCard>(_sortedModels);
        UnsortedModels = new ReadOnlyObservableCollection<ModelCard>(_unsortedModels);
        _appSettingsService = appSettingsService;
        _httpClient = httpClient;

        _sortedModels.CollectionChanged += OnSortedModelsCollectionChanged;
        _unsortedModels.CollectionChanged += OnUnsortedModelsCollectionChanged;

        if (_appSettingsService is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnAppSettingsServicePropertyChanged;
        }
    }

    public void Initialize()
    {
        try
        {
            EnsureModelDirectoryExists();

        }
        catch (Exception)
        {
            // todo
        }

        ModelStorageDirectory = _appSettingsService.ModelStorageDirectory;

#if WINDOWS
        _fileSystemWatcher.Changed += OnFileChanged;
        _fileSystemWatcher.Deleted += OnFileDeleted;
        _fileSystemWatcher.Renamed += OnFileRenamed;
        _fileSystemWatcher.Created += OnFileCreated;

        _fileSystemWatcher.IncludeSubdirectories = true;
        _fileSystemWatcher.EnableRaisingEvents = true;
#endif
    }

#if BETA_DOWNLOAD_MODELS
    public void DownloadModel(ModelCard modelCard)
    {
        var filePath = Path.Combine(ModelStorageDirectory, modelCard.Publisher, modelCard.Repository, modelCard.FileName);

        if (!_fileDownloads.ContainsKey(modelCard.Metadata.DownloadUrl!))
        {
            FileDownloader fileDownloader = new FileDownloader(_httpClient, modelCard.Metadata.DownloadUrl!, filePath);

            fileDownloader.ErrorEventHandler += OnDownloadExceptionThrown;
            fileDownloader.DownloadProgressedEventHandler += OnDownloadProgressed;
            fileDownloader.DownloadCompletedEventHandler += OnDownloadCompleted;

            if (_fileDownloads.TryAdd(modelCard.Metadata.DownloadUrl!, fileDownloader))
            {
                fileDownloader.Start();
            }
        }
    }

    private void ReleaseFileDownloader(Uri downloadUrl)
    {
        if (_fileDownloads.ContainsKey(downloadUrl) && _fileDownloads.Remove(downloadUrl, out FileDownloader? fileDownloader))
        {
            fileDownloader.ErrorEventHandler -= OnDownloadExceptionThrown;
            fileDownloader.DownloadProgressedEventHandler -= OnDownloadProgressed;
            fileDownloader.DownloadCompletedEventHandler -= OnDownloadCompleted;

            fileDownloader.Dispose();
        }
    }

    private void OnDownloadExceptionThrown(Uri downloadUrl, Exception exception)
    {
        ReleaseFileDownloader(downloadUrl);

        if (exception is OperationCanceledException)
        {
            ModelDownloadingCompleted?.Invoke(this, new DownloadOperationStateChangedEventArgs(downloadUrl, DownloadOperationStateChangedEventArgs.DownloadOperationStateChangedType.Canceled));
        }
        else
        {
            ModelDownloadingCompleted?.Invoke(this, new DownloadOperationStateChangedEventArgs(downloadUrl, exception));
        }
    }

    private void OnDownloadProgressed(Uri downloadUrl, long? totalDownloadSize, long byteRead)
    {
        double progress = 0;

        if (totalDownloadSize.HasValue)
        {
            progress = (double)byteRead / totalDownloadSize.Value;
        }

        ModelDownloadingProgressed?.Invoke(this, new DownloadOperationStateChangedEventArgs(downloadUrl, byteRead, totalDownloadSize, progress));
    }

    private void OnDownloadCompleted(Uri downloadUrl)
    {
        ReleaseFileDownloader(downloadUrl);

        ModelDownloadingCompleted?.Invoke(this, new DownloadOperationStateChangedEventArgs(downloadUrl, DownloadOperationStateChangedEventArgs.DownloadOperationStateChangedType.Completed));
    }

    public void CancelModelDownload(ModelCard modelCard)
    {
        if (_fileDownloads.TryGetValue(modelCard.Metadata.DownloadUrl!, out FileDownloader? fileDownloader))
        {
            fileDownloader!.Stop();
        }
    }

    public void PauseModelDownload(ModelCard modelCard)
    {
        if (_fileDownloads.TryGetValue(modelCard.Metadata.DownloadUrl!, out FileDownloader? fileDownloader))
        {
            fileDownloader!.Pause();
        }
    }

    public void ResumeModelDownload(ModelCard modelCard)
    {
        if (_fileDownloads.TryGetValue(modelCard.Metadata.DownloadUrl!, out FileDownloader? fileDownloader))
        {
            fileDownloader!.Resume();
        }
    }
#endif

    public void DeleteModel(ModelCard modelCard)
    {
        if (modelCard.IsLocallyAvailable)
        {
            File.Delete(modelCard.LocalPath);
            DownloadedCount--;
            TotalModelSize -= modelCard.FileSize;

#if !WINDOWS
            if (_unsortedModels.Contains(modelCard))
            {
                _unsortedModels.Remove(modelCard);
            }
            else if (_sortedModels.Contains(modelCard))
            {
                _sortedModels.Remove(modelCard);
            }
#endif
        }
        else
        {
            throw new Exception(TooltipLabels.ModelFileNotAvailableLocally);
        }
    }

    public bool IsPredefinedModel(ModelCard modelCard)
    {
        if (_enablePredefinedModels)
        {
            foreach (var predefinedModel in ModelCard.GetPredefinedModelCards())
            {
                if (modelCard.ModelUri == predefinedModel.ModelUri)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void EnsureModelDirectoryExists()
    {
        if (!Directory.Exists(_appSettingsService.ModelStorageDirectory))
        {
            _appSettingsService.ModelStorageDirectory = LMKitDefaultSettings.DefaultModelStorageDirectory;

            if (!Directory.Exists(_appSettingsService.ModelStorageDirectory))
            {
                if (File.Exists(_appSettingsService.ModelStorageDirectory))
                {
                    File.Delete(_appSettingsService.ModelStorageDirectory);
                }

                Directory.CreateDirectory(_appSettingsService.ModelStorageDirectory);
            }
        }
    }

    private async Task CollectModelsAsync()
    {
        FileCollectingInProgress = true;

        if (FileCollectingInProgress && _cancellationTokenSource != null)
        {
            await CancelOngoingFileCollecting();
        }

        _cancellationTokenSource = new CancellationTokenSource();

        Exception? exception = null;

        bool cancelled = false;

        try
        {
            await (_collectModelFilesTask = Task.Run(() => CollectModels()));
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        if (!cancelled)
        {
            TerminateFileCollectingOperation();
        }

        FileCollectingCompleted?.Invoke(this, new FileCollectingCompletedEventArgs(exception == null, exception));
    }

    private async Task CancelOngoingFileCollecting()
    {
        try
        {
            _cancellationTokenSource!.Cancel();
            await _collectModelFilesTask!.ConfigureAwait(false);
        }
        catch
        {
            return;
        }
    }

    private void TerminateFileCollectingOperation()
    {
        FileCollectingInProgress = false;
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _collectModelFilesTask = null;
    }

    private void CollectModels()
    {
        if (_enablePredefinedModels)
        {
            foreach (var modelCard in ModelCard.GetPredefinedModelCards())
            {
                TryRegisterChatModel(modelCard);

                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();
            }
        }

        if (_enableCustomModels)
        {
            var files = Directory.GetFileSystemEntries(ModelStorageDirectory, "*", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                if (ShouldCheckFile(filePath))
                {
                    HandleFile(filePath);
                }

                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();
            }
        }
    }

    private bool HasModel(ModelCard? modelCard)
    {
        if (modelCard != null)
        {
            foreach (var model in _sortedModels)
            {
                /*if (model.SHA256 == modelCard.SHA256) //Loïc: commented. This is too slow.
                {
                    return true;
                }*/

                if (model.ModelUri == modelCard.ModelUri ||
                    model.FileName == modelCard.FileName && model.FileSize == modelCard.FileSize)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool TryRegisterChatModel(ModelCard? modelCard)
    {
        if (modelCard != null)
        {
            if (!modelCard.Capabilities.HasFlag(ModelCapabilities.Chat))
            {
                return false;
            }

            if (!HasModel(modelCard))
            {
                _sortedModels.Add(modelCard);
                return true;
            }
        }

        return false;
    }

    private void HandleFile(string filePath, bool collectAll = true)
    {
        if (!TryValidateModelFile(filePath, ModelStorageDirectory, out ModelCard? modelCard, out bool isSorted))
        {
            return;
        }

        if (isSorted)
        {
            TryRegisterChatModel(modelCard);
        }
        else
        {
            Uri fileUri = new Uri(filePath);

            modelCard = new ModelCard(new Uri(filePath))
            {
                Publisher = "unknown publisher",
                Repository = "unknown repository"
            };

            if (!ModelListContainsFileUri(_unsortedModels, fileUri, out _))
            {
                _unsortedModels.Add(modelCard);
            }

            if (collectAll)
            {
                TryRegisterChatModel(modelCard);
            }
        }
    }

    private void HandleFileRecording(Uri fileUri)
    {
        var fileRecord = _fileSystemEntryRecorder.RecordFile(fileUri);

        if (fileRecord != null)
        {
            fileRecord.FilePathChanged += OnFileRecordPathChanged;
        }
    }

    private void HandleFileRecordDeletion(Uri fileUri)
    {
        var fileRecord = _fileSystemEntryRecorder.DeleteFileRecord(fileUri!);

        if (fileRecord != null)
        {
            fileRecord.FilePathChanged -= OnFileRecordPathChanged;
        }
    }

    #region Event handlers

    private async void OnModelStorageDirectoryChanged()
    {
        if (FileCollectingInProgress)
        {
            _cancellationTokenSource?.Cancel();
        }

        _fileSystemEntryRecorder.Init(_modelStorageDirectory);

        _unsortedModels.Clear();
        _sortedModels.Clear();

        await CollectModelsAsync();
    }

    private void OnAppSettingsServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IAppSettingsService.ModelStorageDirectory))
        {
            ModelStorageDirectory = _appSettingsService.ModelStorageDirectory;
        }
    }

#if WINDOWS
    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        Uri fileUri = new Uri(e.FullPath);

        if (ModelListContainsFileUri(_unsortedModels, fileUri, out int index))
        {
            _unsortedModels.RemoveAt(index);
        }
        else if (ModelListContainsFileUri(_sortedModels, fileUri, out index))
        {
            if (!IsPredefinedModel(_sortedModels[index]))
            {
                _sortedModels.RemoveAt(index);
            }
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.Name != null)
        {
            bool shouldCheckFile = ShouldCheckFile(e.FullPath);

            if (shouldCheckFile)
            {
                bool accessGranted = WaitFileReadAccessGranted(e.FullPath);

                if (accessGranted)
                {
                    HandleFile(e.FullPath);
                }
            }
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (ShouldCheckFile(e.FullPath) && WaitFileReadAccessGranted(e.FullPath))
        {
            HandleFile(e.FullPath);
        }
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        var entryRecord = _fileSystemEntryRecorder.TryGetExistingEntry(new Uri(e.OldFullPath));

        if (entryRecord != null)
        {
            entryRecord.Rename(FileHelpers.GetFileBaseName(new Uri(e.FullPath)));
        }
        else
        {
            if (ShouldCheckFile(e.FullPath))
            {
                HandleFile(e.FullPath);
            }
        }

    }
#endif

#if BETA_DOWNLOAD_MODELS
    private void OnModelDownloadingProgressed(string path, long? contentLength, long bytesRead)
    {
        double progress = 0;

        if (contentLength.HasValue)
        {
            double progressPercentage = Math.Round((double)bytesRead / contentLength.Value * 100, 2);

            progress = (double)bytesRead / contentLength.Value;
            //Console.Write($"\rDownloading model {progressPercentage:0.00}%");
        }
        else
        {
            //Console.Write($"\rDownloading model {bytesRead} bytes");
        }

        if (ModelDownloadingProgressed != null)
        {
            ModelDownloadingProgressedEventArgs eventArgs = new ModelDownloadingProgressedEventArgs(path, bytesRead, contentLength, progress);
            ModelDownloadingProgressed.Invoke(this, eventArgs);
        }
    }
#endif

    private void OnSortedModelsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems!)
            {
                var card = ((ModelCard)item);

                if (card.IsLocallyAvailable)
                {
                    TotalModelSize += card.FileSize;
                    DownloadedCount++;
                }

                HandleFileRecording(card.ModelUri!);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (var item in e.OldItems!)
            {
                var card = ((ModelCard)item);

                if (card.IsLocallyAvailable)
                {
                    TotalModelSize -= card.FileSize;
                    DownloadedCount--;
                }

                HandleFileRecordDeletion(card.ModelUri!);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            TotalModelSize = 0;
            DownloadedCount = 0;
        }

        SortedModelCollectionChanged?.Invoke(sender, e);
    }

    private void OnUnsortedModelsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems!)
            {
                HandleFileRecording(((ModelCard)item).ModelUri!);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (var item in e.OldItems!)
            {
                HandleFileRecordDeletion(((ModelCard)item).ModelUri!);
            }
        }
    }

    private void OnFileRecordPathChanged(object? sender, EventArgs e)
    {
        var fileRecordPathChangedEventArgs = (FileSystemEntryRecorder.FileRecordPathChangedEventArgs)e;

        if (ModelListContainsFileUri(_unsortedModels, fileRecordPathChangedEventArgs.OldPath, out int index))
        {
            _unsortedModels[index] = new ModelCard(fileRecordPathChangedEventArgs.NewPath)
            {
                Publisher = _unsortedModels[index].Publisher,
                Repository = _unsortedModels[index].Repository
            };
        }
        else if (ModelListContainsFileUri(_sortedModels, fileRecordPathChangedEventArgs.OldPath, out index) &&
                FileHelpers.GetModelInfoFromFileUri(fileRecordPathChangedEventArgs.NewPath, ModelStorageDirectory,
                out string publisher, out string repository, out string fileName))
        {
            _sortedModels[index] = new ModelCard(fileRecordPathChangedEventArgs.NewPath)
            {
                Publisher = publisher,
                Repository = repository,
            };
        }
    }
    #endregion

    #region Static methods


    private static bool TryValidateModelFile(string filePath, string modelFolderPath, out ModelCard? modelCard, out bool isSorted)
    {
        isSorted = false;
        modelCard = null;

        if (LLM.ValidateFormat(filePath))
        {
            try
            {
                modelCard = ModelCard.CreateFromFile(filePath);
            }
            catch
            {
                return false;
            }

            if (FileHelpers.GetModelInfoFromPath(filePath, modelFolderPath,
                out string publisher, out string repository, out string fileName))
            {
                isSorted = true;
#if BETA_DOWNLOAD_MODELS
                modelCard = TryGetExistingModelInfo(fileName, repository, publisher);
                if (modelCard == null)
                {
                    modelCard = new ModelInfo(publisher, repository, fileName);
                    modelCard.Metadata.FileSize = FileHelpers.GetFileSize(filePath);
                }

                modelCard.Metadata.FileUri = new Uri(filePath);

#else
                modelCard.Publisher = publisher;
                modelCard.Repository = repository;

#endif
            }

            return true;
        }

        return false;
    }

#if BETA_DOWNLOAD_MODELS
    private static ModelInfo? TryGetExistingModelInfo(string fileName, string repository, string publisher)
    {
        foreach (var modelCard in AppConstants.AvailableModels)
        {
            if (string.CompareOrdinal(modelCard.FileName, fileName) == 0 &&
                string.CompareOrdinal(modelCard.Repository, repository) == 0 &&
                string.CompareOrdinal(modelCard.Publisher, publisher) == 0)
            {
                return modelCard;
            }
        }

        return null;
    }
#endif

    private static bool ShouldCheckFile(string filePath)
    {
        bool isFileDirectory = FileHelpers.IsFileDirectory(filePath);

        if (isFileDirectory)
        {
            return false;
        }
        else
        {
            return !(filePath.EndsWith(".download")) &&
                   !(filePath.EndsWith(".origin"));
        }
    }

    private static bool WaitFileReadAccessGranted(string fileName, int maxRetryCount = 3)
    {
        for (int retryCount = 0; retryCount < maxRetryCount; retryCount++)
        {
            if (!FileHelpers.IsFileLocked(fileName))
            {
                return true;
            }
            else
            {
                if (retryCount + 1 < maxRetryCount)
                {
                    Thread.Sleep(2000);
                }
            }
        }

        return false;
    }

    private static bool ModelListContainsFileUri(IList<ModelCard> models, Uri fileUri, out int matchIndex)
    {
        for (int index = 0; index < models.Count; index++)
        {
            ModelCard modelCard = models[index];

            if (modelCard.ModelUri! == fileUri)
            {
                matchIndex = index;
                return true;
            }
        }

        matchIndex = -1;
        return false;
    }

    #endregion
}
