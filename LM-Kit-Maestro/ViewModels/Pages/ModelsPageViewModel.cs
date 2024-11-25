using LMKit.Maestro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Interfaces;
using CommunityToolkit.Maui.Storage;

namespace LMKit.Maestro.ViewModels;

public partial class ModelsPageViewModel : PageViewModelBase
{
    private readonly IFolderPicker _folderPicker;
    private readonly IPopupService _popupService;
    private readonly Services.ILauncher _launcher;
    private readonly LMKitService _lmKitService;
    private readonly IMainThread _mainThread;

    public ILLMFileManager FileManager { get; }
    public IAppSettingsService AppSettingsService { get; }
    public ModelListViewModel ModelListViewModel { get; }

    [ObservableProperty] long _totalModelSize;

    public ModelsPageViewModel(INavigationService navigationService, IFolderPicker folderPicker,
        Services.ILauncher launcher, IMainThread mainThread,
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
    public void DownloadModel(ModelInfoViewModel modelInfoViewModel)
    {
        modelInfoViewModel.DownloadInfo.Status = DownloadStatus.Downloading;
        modelInfoViewModel.ModelInfo.Metadata.FileUri =
 FileHelpers.GetModelFileUri(modelInfoViewModel.ModelInfo, AppSettingsService.ModelsFolderPath);

        FileManager.DownloadModel(modelInfoViewModel.ModelInfo);
    }

    [RelayCommand]
    public void CancelDownload(ModelInfoViewModel modelInfoViewModel)
    {
        modelInfoViewModel.DownloadInfo.Status = DownloadStatus.NotDownloaded;

        FileManager.CancelModelDownload(modelInfoViewModel.ModelInfo);
    }

    [RelayCommand]
    public void PauseDownload(ModelInfoViewModel modelInfoViewModel)
    {
        modelInfoViewModel.DownloadInfo.Status = DownloadStatus.DownloadPaused;

        FileManager.PauseModelDownload(modelInfoViewModel.ModelInfo);
    }

    [RelayCommand]
    public void ResumeDownload(ModelInfoViewModel modelInfoViewModel)
    {
        modelInfoViewModel.DownloadInfo.Status = DownloadStatus.Downloading;

        FileManager.ResumeModelDownload(modelInfoViewModel.ModelInfo);
    }
#endif

    [RelayCommand]
    public void DeleteModel(ModelInfoViewModel modelInfoViewModel)
    {
        try
        {
            FileManager.DeleteModel(modelInfoViewModel.ModelInfo);
        }
        catch (Exception ex)
        {
            _popupService.DisplayAlert("Failure to delete model file",
                $"The model file could not be deleted:\n {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void PickModelsFolder()
    {
        _mainThread.BeginInvokeOnMainThread(async () =>
        {
            var result = await _folderPicker.PickAsync(AppSettingsService.ModelsFolderPath);

            if (result.IsSuccessful)
            {
                if (_lmKitService.ModelLoadingState != LMKitModelLoadingState.Unloaded)
                {
                    _lmKitService.UnloadModel();
                }

                AppSettingsService.ModelsFolderPath = result.Folder.Path!;
            }
        });
    }

    [RelayCommand]
    public async Task OpenModelsFolder()
    {
        try
        {
            await _launcher.OpenAsync(new Uri(AppSettingsService.ModelsFolderPath));
        }
        catch
        {
        }
    }

    [RelayCommand]
    public async Task OpenHuggingFaceLink()
    {
        try
        {
            await _launcher.OpenAsync(new Uri("https://huggingface.co/lm-kit"));
        }
        catch
        {
        }
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