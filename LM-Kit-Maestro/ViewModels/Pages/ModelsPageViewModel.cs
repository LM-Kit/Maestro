using LMKit.Maestro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Interfaces;
using CommunityToolkit.Maui.Storage;
using System.Diagnostics;

namespace LMKit.Maestro.ViewModels;

public partial class ModelsPageViewModel : PageViewModelBase
{
    private readonly IFolderPicker _folderPicker;
    private readonly IPopupService _popupService;
    private readonly ILauncher _launcher;
    private readonly LMKitService _lmKitService;
    private readonly IMainThread _mainThread;

    public ILLMFileManager FileManager { get; }
    public IAppSettingsService AppSettingsService { get; }
    public ModelListViewModel ModelListViewModel { get; }

    [ObservableProperty] long _totalModelSize;

    public ModelsPageViewModel(INavigationService navigationService, IFolderPicker folderPicker,
        ILauncher launcher, IMainThread mainThread,
        IPopupService popupService, IPopupNavigation popupNavigation, ILLMFileManager llmFileManager,
        LMKitService lmKitService,
        IAppSettingsService appSettingsService, ModelListViewModel modelListViewModel) : base(navigationService,
        popupService, popupNavigation)
    {
        _folderPicker = folderPicker;
        _launcher = launcher;
        _mainThread = mainThread;
        _lmKitService = lmKitService;
        _popupService = popupService;
        FileManager = llmFileManager;
        ModelListViewModel = modelListViewModel;
        AppSettingsService = appSettingsService;

#if MODEL_DOWNLOAD
        llmFileManager.ModelDownloadingProgressed += OnModelDownloadingProgressed;
        llmFileManager.ModelDownloadingCompleted += OnModelDownloadingCompleted;
#endif
    }

#if MODEL_DOWNLOAD
    [RelayCommand]
    public void DownloadModel(ModelInfoViewModel modelCardViewModel)
    {
        modelCardViewModel.DownloadInfo.Status = DownloadStatus.Downloading;
        modelCardViewModel.ModelInfo.Metadata.FileUri =
 FileHelpers.GetModelFileUri(modelCardViewModel.ModelInfo, AppSettingsService.ModelStorageDirectory);

        FileManager.DownloadModel(modelCardViewModel.ModelInfo);
    }

    [RelayCommand]
    public void CancelDownload(ModelInfoViewModel modelCardViewModel)
    {
        modelCardViewModel.DownloadInfo.Status = DownloadStatus.NotDownloaded;

        FileManager.CancelModelDownload(modelCardViewModel.ModelInfo);
    }

    [RelayCommand]
    public void PauseDownload(ModelInfoViewModel modelCardViewModel)
    {
        modelCardViewModel.DownloadInfo.Status = DownloadStatus.DownloadPaused;

        FileManager.PauseModelDownload(modelCardViewModel.ModelInfo);
    }

    [RelayCommand]
    public void ResumeDownload(ModelInfoViewModel modelCardViewModel)
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
            var result = await _folderPicker.PickAsync(AppSettingsService.ModelStorageDirectory);

            if (result.IsSuccessful)
            {
                if (_lmKitService.ModelLoadingState != LMKitModelLoadingState.Unloaded)
                {
                    _lmKitService.UnloadModel();
                }

                AppSettingsService.ModelStorageDirectory = result.Folder.Path!;
            }
        });
#endif
    }

    [RelayCommand]
    public void OpenModelsFolder()
    {
        _ = _launcher.OpenAsync(new Uri($"file://{AppSettingsService.ModelStorageDirectory}"));
    }

    [RelayCommand]
    public void ResetModelsFolder()
    {
        AppSettingsService.ModelStorageDirectory = LMKitDefaultSettings.DefaultModelStorageDirectory;
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

        var modelViewModel = MaestroHelpers.TryGetExistingModelInfoViewModel(ModelListViewModel.AvailableModels,
            downloadOperationStateChangedEventArgs.DownloadUrl)!;

        modelViewModel!.DownloadInfo.Progress = downloadOperationStateChangedEventArgs.Progress;
        modelViewModel.DownloadInfo.BytesRead = downloadOperationStateChangedEventArgs.BytesRead;
        modelViewModel.DownloadInfo.ContentLength = downloadOperationStateChangedEventArgs.ContentLength;
    }

    private async void OnModelDownloadingCompleted(object? sender, EventArgs e)
    {
        var downloadOperationStateChangedEventArgs = (DownloadOperationStateChangedEventArgs)e;

        var modelViewModel = MaestroHelpers.TryGetExistingModelInfoViewModel(ModelListViewModel.AvailableModels,
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