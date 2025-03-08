using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Helpers;
using LMKit.Maestro.Services;
using static LMKit.Maestro.Services.LLMFileManager;

namespace LMKit.Maestro.ViewModels;

public partial class ModelsPageViewModel : ViewModelBase
{
    private readonly IFolderPicker _folderPicker;
    private readonly ILauncher _launcher;
    private readonly LMKitService _lmKitService;
    private readonly IMainThread _mainThread;

    public ILLMFileManager FileManager { get; }
    public IAppSettingsService AppSettingsService { get; }
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
        AppSettingsService = appSettingsService;

    }

    public void DownloadModel(ModelInfoViewModel modelCardViewModel)
    {
        modelCardViewModel.DownloadInfo.Status = DownloadStatus.Downloading;

        //FileManager.DownloadModel(modelCardViewModel.ModelInfo);
    }

    public void CancelDownload(ModelInfoViewModel modelCardViewModel)
    {
        modelCardViewModel.DownloadInfo.Status = DownloadStatus.NotDownloaded;

        //FileManager.CancelModelDownload(modelCardViewModel.ModelInfo);
    }

    public void PauseDownload(ModelInfoViewModel modelCardViewModel)
    {
        modelCardViewModel.DownloadInfo.Status = DownloadStatus.DownloadPaused;

        //FileManager.PauseModelDownload(modelCardViewModel.ModelInfo);
    }

    public void ResumeDownload(ModelInfoViewModel modelCardViewModel)
    {
        modelCardViewModel.DownloadInfo.Status = DownloadStatus.Downloading;
        //FileManager.ResumeModelDownload(modelCardViewModel.ModelInfo);
    }

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

    public void OpenModelsFolder()
    {
        _ = _launcher.OpenAsync(new Uri($"file://{AppSettingsService.ModelStorageDirectory}"));
    }

    public void ResetModelsFolder()
    {
        AppSettingsService.ModelStorageDirectory = LMKitDefaultSettings.DefaultModelStorageDirectory;
    }

    public void OpenHuggingFaceLink()
    {
        _ = _launcher.OpenAsync(new Uri("https://huggingface.co/lm-kit"));
    }

    private void OnModelDownloadingProgressed(object? sender, EventArgs e)
    {
        var downloadOperationStateChangedEventArgs = (DownloadOperationStateChangedEventArgs)e;

        var modelViewModel = MaestroHelpers.TryGetExistingModelInfoViewModel(ModelListViewModel.Models,
            downloadOperationStateChangedEventArgs.ModelCard)!;

        modelViewModel!.DownloadInfo.Progress = downloadOperationStateChangedEventArgs.Progress;
        modelViewModel.DownloadInfo.BytesRead = downloadOperationStateChangedEventArgs.BytesRead;
        modelViewModel.DownloadInfo.ContentLength = downloadOperationStateChangedEventArgs.ContentLength;
    }

    private async void OnModelDownloadingCompleted(object? sender, EventArgs e)
    {
        var downloadOperationStateChangedEventArgs = (DownloadOperationStateChangedEventArgs)e;

        var modelViewModel = MaestroHelpers.TryGetExistingModelInfoViewModel(ModelListViewModel.Models,
            downloadOperationStateChangedEventArgs.ModelCard)!;

        if (downloadOperationStateChangedEventArgs.Exception != null)
        {
            modelViewModel.DownloadInfo.Status = DownloadStatus.NotDownloaded;
            //    await MaestroHelpers.DisplayError("Model download failure",
            //        $"Download of model '{modelViewModel.Name}' failed:\n{downloadOperationStateChangedEventArgs.Exception.Message}");
            //}
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
}