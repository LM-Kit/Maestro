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
}