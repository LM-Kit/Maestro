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

    private readonly FileSystemEntryRecorder _fileSystemEntryRecorder;
    private readonly IAppSettingsService _appSettingsService;
    private readonly HttpClient _httpClient;
    private bool _enablePredefinedModels = true; //todo: Implement this as a configurable option in the configuration panel 
    //todo: make this user-configurable is some way.
    private List<ModelCapabilities> _filteredCapabilities = new List<ModelCapabilities>() { ModelCapabilities.Chat,
                                                                                            ModelCapabilities.Math,
                                                                                            ModelCapabilities.CodeCompletion };
    private bool _enableCustomModels = true;
    private bool _isLoaded = false;

    private readonly Dictionary<Uri, FileDownloader> _fileDownloads = new Dictionary<Uri, FileDownloader>();

    private delegate bool ModelDownloadingProgressCallback(string path, long? contentLength, long bytesRead);
    public event NotifyCollectionChangedEventHandler? SortedModelCollectionChanged;

    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _collectModelFilesTask;

    public ReadOnlyObservableCollection<ModelCard> Models { get; }
    public ReadOnlyObservableCollection<ModelCard> UnsortedModels { get; }

    private ObservableCollection<ModelCard> _models { get; } = new ObservableCollection<ModelCard>();
    private ObservableCollection<ModelCard> _unsortedModels { get; } = new ObservableCollection<ModelCard>();

    [ObservableProperty]
    private long _totalModelSize;

    [ObservableProperty]
    private int _downloadedCount;

    [ObservableProperty]
    private bool _fileCollectingInProgress;

    [ObservableProperty]
    private bool _enableLowPerformanceModels;


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
        Models = new ReadOnlyObservableCollection<ModelCard>(_models);
        UnsortedModels = new ReadOnlyObservableCollection<ModelCard>(_unsortedModels);
        _appSettingsService = appSettingsService;
        _enableLowPerformanceModels = _appSettingsService.EnableLowPerformanceModels;
        _httpClient = httpClient;
        _models.CollectionChanged += OnModelCollectionChanged;
        _unsortedModels.CollectionChanged += OnUnsortedModelCollectionChanged;

        if (_appSettingsService is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnAppSettingsServicePropertyChanged;
        }

        try
        {
            EnsureModelDirectoryExists();
        }
        catch (Exception)
        {
            // todo
        }

        ModelStorageDirectory = _appSettingsService.ModelStorageDirectory;
        _fileSystemEntryRecorder = new FileSystemEntryRecorder(new Uri(ModelStorageDirectory));
        _isLoaded = true;
    }

    public void Initialize()
    {
#if WINDOWS
        _fileSystemWatcher.Changed += OnFileChanged;
        _fileSystemWatcher.Deleted += OnFileDeleted;
        _fileSystemWatcher.Renamed += OnFileRenamed;
        _fileSystemWatcher.Created += OnFileCreated;

        _fileSystemWatcher.IncludeSubdirectories = true;
        _fileSystemWatcher.EnableRaisingEvents = true;
#endif

        _ = CollectModelsAsync();
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

    public void OnModelDownloaded(ModelCard modelCard)
    {
        TotalModelSize += modelCard.FileSize;
        DownloadedCount++;
    }

    public void DeleteModel(ModelCard modelCard)
    {
        if (modelCard.IsLocallyAvailable)
        {
            File.Delete(modelCard.LocalPath);
            DownloadedCount--;
            TotalModelSize -= modelCard.FileSize;

#if !WINDOWS
            if (!modelCard.IsPredefined)
            {
                if (_unsortedModels.Contains(modelCard))
                {
                    _unsortedModels.Remove(modelCard);
                }

                if (_models.Contains(modelCard))
                {
                    _models.Remove(modelCard);
                }                
            }
#endif
        }
        else
        {
            throw new Exception(TooltipLabels.ModelFileNotAvailableLocally);
        }
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
            var predefinedModels = ModelCard.GetPredefinedModelCards(dropSmallerModels: !EnableLowPerformanceModels);

            if (_models.Count > 0)
            {
                for (int index = 0; index < _models.Count; index++)
                {
                    if (_models[index].IsPredefined && !predefinedModels.Contains(_models[index]))
                    {
                        _models.RemoveAt(index);
                        index--;
                    }
                }
            }

            foreach (var modelCard in ModelCard.GetPredefinedModelCards(dropSmallerModels: !EnableLowPerformanceModels))
            {
                TryRegisterChatModel(modelCard, isSorted: true);

                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();
            }
        }

        if (_enableCustomModels)
        {
            var files = Directory.GetFileSystemEntries(ModelStorageDirectory, "*", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                bool processed = false;

                foreach (var predefinedModel in ModelCard.GetPredefinedModelCards(dropSmallerModels: false))
                {
                    if (predefinedModel.LocalPath == filePath)
                    {//Skip this model because it has already been processed
                        processed = true;
                        break;
                    }
                }

                if (processed)
                {
                    continue;
                }

                if (ShouldCheckFile(filePath))
                {
                    HandleFile(filePath);
                }

                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();
            }
        }
    }

    private bool TryRegisterChatModel(ModelCard? modelCard, bool isSorted)
    {
        if (modelCard != null)
        {
            bool hasAnyFilteredCap = false;

            foreach (var cap in _filteredCapabilities)
            {
                if (modelCard.Capabilities.HasFlag(cap))
                {
                    hasAnyFilteredCap = true;
                    break;
                }
            }

            if (!hasAnyFilteredCap)
            {
                return false;
            }

            bool isSlowModel = Graphics.DeviceConfiguration.GetPerformanceScore(modelCard) < 0.3;

            if (!ContainsModel(_models, modelCard))
            {
                if (isSlowModel && !EnableLowPerformanceModels)
                {
                    return false;
                }

                _models.Add(modelCard);

                if (!isSorted)
                {
                    _unsortedModels.Add(modelCard);
                }

                return true;
            }
            else if (isSlowModel && !EnableLowPerformanceModels)
            {
                _models.Remove(modelCard);
            }
        }

        return false;
    }

    private void HandleFile(string filePath)
    {
        if (!TryValidateModelFile(filePath, ModelStorageDirectory, out ModelCard? modelCard, out bool isSorted))
        {
            //todo: Add feedback to indicate that a model of a supported format could not be loaded
            return;
        }

        TryRegisterChatModel(modelCard, isSorted);
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

        if (_isLoaded)
        {
            _fileSystemEntryRecorder.Update(new Uri(_modelStorageDirectory));

            _unsortedModels.Clear();
            _models.Clear();

            await CollectModelsAsync();
        }
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

        if (ContainsModel(_models, fileUri, out int index))
        {
            if (!_models[index].IsPredefined)
            {
                var model = _models[index];
                _models.Remove(model);

                if (_unsortedModels.Contains(model))
                {
                    _unsortedModels.Remove(model);
                }
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

    partial void OnEnableLowPerformanceModelsChanged(bool value)
    {
        _appSettingsService.EnableLowPerformanceModels = value;
        _ = CollectModelsAsync();
    }

    private void OnModelCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

    private void OnUnsortedModelCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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


        if (ContainsModel(_models, fileRecordPathChangedEventArgs.OldPath, out int index) &&
            FileHelpers.GetModelInfoFromFileUri(fileRecordPathChangedEventArgs.NewPath,
                                                ModelStorageDirectory,
                                                out string publisher,
                                                out string repository,
                                                out string fileName))
        {
            var prevModel = _models[index];
            _models[index] = new ModelCard(fileRecordPathChangedEventArgs.NewPath)
            {
                Publisher = publisher,
                Repository = repository,
            };

            if (_unsortedModels.Contains(prevModel))
            {
                _unsortedModels[_unsortedModels.IndexOf(prevModel)] = _models[index];
            }
        }
    }
    #endregion

    #region Static methods

    private static bool ContainsModel(IList<ModelCard> models, ModelCard modelCard)
    {
        if (models.Contains(modelCard))
        {
            return true;
        }

        //In this scope, we are essentially searching for duplicate model files..
        foreach (var model in models)
        {
            /*if (model.SHA256 == modelCard.SHA256) //Loïc: commented. This is too slow.
            {
                return true;
            }*/

            if (model.ModelUri == modelCard.ModelUri ||
                model.FileName == modelCard.FileName && model.FileSize == modelCard.FileSize)
            {
                if (model.LocalPath == modelCard.LocalPath)
                {
                    return true;
                }
                else if (model.SHA256 == modelCard.SHA256)
                {
                    //todo: propagate feedback indicating that a duplicate file exists.
                    return true;
                }
            }
        }

        return false;
    }

    private static bool ContainsModel(IList<ModelCard> models, Uri uri, out int index)
    {
        index = 0;

        foreach (var model in models)
        {
            if (model.ModelUri == uri)
            {
                return true;
            }

            index++;
        }

        index = -1;

        return false;
    }

    private static bool TryValidateModelFile(string filePath, string modelFolderPath, out ModelCard? modelCard, out bool isSorted)
    {
        isSorted = false;
        modelCard = null;

        if (LM.ValidateFormat(filePath))
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
            else
            {
                modelCard.Publisher = "unknown publisher";
                modelCard.Repository = "unknown repository";
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

    #endregion
}
