using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Services;

namespace LMKit.Maestro.ViewModels;

public partial class ModelsPageViewModel : ViewModelBase
{
    private readonly IFolderPicker _folderPicker;
    private readonly ILauncher _launcher;
    private readonly LMKitService _lmKitService;
    private readonly IMainThread _mainThread;

    public ILLMFileManager FileManager { get; }
    public ModelListViewModel ModelListViewModel { get; }

    [ObservableProperty] long _totalModelSize;

    public ModelsPageViewModel(IFolderPicker folderPicker,
        ILauncher launcher, IMainThread mainThread, ILLMFileManager llmFileManager,
        LMKitService lmKitService,
        IAppSettingsService appSettingsService, ModelListViewModel modelListViewModel)
    {
        _folderPicker = folderPicker;
        _launcher = launcher;
        _mainThread = mainThread;
        _lmKitService = lmKitService;
        FileManager = llmFileManager;
        ModelListViewModel = modelListViewModel;

#if MODEL_DOWNLOAD
        llmFileManager.ModelDownloadingProgressed += OnModelDownloadingProgressed;
        llmFileManager.ModelDownloadingCompleted += OnModelDownloadingCompleted;
#endif
    }

#if MODEL_DOWNLOAD
    [RelayCommand]
    public void DownloadModel(ModelCardViewModel modelCardViewModel)
    {
        modelCardViewModel.DownloadInfo.Status = DownloadStatus.Downloading;
        modelCardViewModel.ModelInfo.Metadata.FileUri =
 FileHelpers.GetModelFileUri(modelCardViewModel.ModelInfo, AppSettingsService.ModelStorageDirectory);

        FileManager.DownloadModel(modelCardViewModel.ModelInfo);
    }

    [RelayCommand]
    public void CancelDownload(ModelCardViewModel modelCardViewModel)
    {
        modelCardViewModel.DownloadInfo.Status = DownloadStatus.NotDownloaded;

        FileManager.CancelModelDownload(modelCardViewModel.ModelInfo);
    }

    [RelayCommand]
    public void PauseDownload(ModelCardViewModel modelCardViewModel)
    {
        modelCardViewModel.DownloadInfo.Status = DownloadStatus.DownloadPaused;

        FileManager.PauseModelDownload(modelCardViewModel.ModelInfo);
    }

    [RelayCommand]
    public void ResumeDownload(ModelCardViewModel modelCardViewModel)
    {
        modelCardViewModel.DownloadInfo.Status = DownloadStatus.Downloading;

        FileManager.ResumeModelDownload(modelCardViewModel.ModelInfo);
    }
#endif

    [RelayCommand]
    public void PickModelsFolder()
    {
#if MACCATALYST  || WINDOWS
        _mainThread.BeginInvokeOnMainThread(async () =>
        {
            var result = await _folderPicker.PickAsync(ModelListViewModel.ModelsSettings.ModelsDirectory);

            if (result.IsSuccessful)
            {
                if (_lmKitService.ModelLoadingState != LMKitModelLoadingState.Unloaded)
                {
                    _lmKitService.UnloadModel();
                }

                ModelListViewModel.ModelsSettings.ModelsDirectory = result.Folder.Path!;
            }
        });
#endif
    }

    [RelayCommand]
    public void OpenModelsFolder()
    {
        _ = _launcher.OpenAsync(new Uri($"file://{ModelListViewModel.ModelsSettings.ModelsDirectory}"));
    }

    [RelayCommand]
    public void ResetModelsFolder()
    {
        ModelListViewModel.ModelsSettings.ModelsDirectory = LMKitDefaultSettings.DefaultModelStorageDirectory;
    }

    [RelayCommand]
    public void OpenHuggingFaceLink()
    {
        _ = _launcher.OpenAsync(new Uri("https://huggingface.co/lm-kit"));
    }

#if BETA_DOWNLOAD_MODELS
    private void OnModelDownloadingProgressed(object? sender, EventArgs e)
    {
        var downloadOperationStateChangedEventArgs = (DownloadOperationStateChangedEventArgs)e;

        var modelViewModel = MaestroHelpers.TryGetExistingModelCardViewModel(ModelListViewModel.AvailableModels,
            downloadOperationStateChangedEventArgs.DownloadUrl)!;

        modelViewModel!.DownloadInfo.Progress = downloadOperationStateChangedEventArgs.Progress;
        modelViewModel.DownloadInfo.BytesRead = downloadOperationStateChangedEventArgs.BytesRead;
        modelViewModel.DownloadInfo.ContentLength = downloadOperationStateChangedEventArgs.ContentLength;
    }

    private async void OnModelDownloadingCompleted(object? sender, EventArgs e)
    {
        var downloadOperationStateChangedEventArgs = (DownloadOperationStateChangedEventArgs)e;

        var modelViewModel = MaestroHelpers.TryGetExistingModelCardViewModel(ModelListViewModel.AvailableModels,
            downloadOperationStateChangedEventArgs.DownloadUrl)!;

        if (downloadOperationStateChangedEventArgs.Exception != null)
        {
            modelViewModel.DownloadInfo.Status = DownloadStatus.NotDownloaded;
            await MaestroHelpers.DisplayError("Model download failure",
                $"Download of model '{modelViewModel.Name}' failed:\n{downloadOperationStateChangedEventArgs.Exception.Message}");
        }
        else if (downloadOperationStateChangedEventArgs.Type == DownloadOperationStateChangedEventArgs.DownloadOperationStateChangedType.Canceled)
        {
            modelViewModel.DownloadInfo.Status = DownloadStatus.NotDownloaded;
        }
        else if (downloadOperationStateChangedEventArgs.Type == DownloadOperationStateChangedEventArgs.DownloadOperationStateChangedType.Completed)
        {
            modelViewModel.DownloadInfo.Status = DownloadStatus.Downloaded;
        }

        modelViewModel.DownloadInfo.Progress = 0;
        modelViewModel.DownloadInfo.BytesRead = 0;
    }
#endif
}