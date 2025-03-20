using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Services;
using Microsoft.Extensions.Logging;

namespace LMKit.Maestro.ViewModels;

public partial class MaestroViewModel : ViewModelBase
{
    private readonly ISnackbarService _snackbarService;
    private readonly ILogger<MaestroViewModel> _logger;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly ModelListViewModel _modelListViewModel;
    private readonly ConversationListViewModel _conversationListViewModel;
    private readonly LMKitService _lmKitService;
    private readonly ILLMFileManager _llmFileManager;
    private readonly IAppSettingsService _appSettingsService;

    [ObservableProperty]
    private bool _appIsInitialized = false;

    public MaestroViewModel(ISnackbarService snackbarService, ILogger<MaestroViewModel> logger,
        ConversationListViewModel conversationListViewModel, ModelListViewModel modelListViewModel,
        SettingsViewModel settingsViewModel, LMKitService lmKitService,
        ILLMFileManager llmFileManager, IAppSettingsService appSettingsService)
    {
        _snackbarService = snackbarService;
        _logger = logger;
        _conversationListViewModel = conversationListViewModel;
        _modelListViewModel = modelListViewModel;
        _settingsViewModel = settingsViewModel;
        _lmKitService = lmKitService;
        _llmFileManager = llmFileManager;
        _appSettingsService = appSettingsService;
    }

    public async Task Init()
    {
        _settingsViewModel.Init();

        await _conversationListViewModel.LoadConversationLogs();

        EnsureModelDirectoryExists();
        _llmFileManager.FileCollectingCompleted += OnFileManagerFileCollectingCompleted;

        _lmKitService.ModelLoadingFailed += OnModelLoadingFailed;

        if (_appSettingsService.LastLoadedModelUri != null)
        {
            _ = Task.Run(TryLoadLastUsedModel); //Loading model in the background to avoid blocking UI initialization.
        }

        // todo: we should ensure UI is loaded before starting loading a model with this call.
        _modelListViewModel.Initialize();

        AppIsInitialized = true;
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
    private void TryLoadLastUsedModel()
    {
        if (_appSettingsService.LastLoadedModelUri != null)
        {
            _lmKitService.LoadModel(_appSettingsService.LastLoadedModelUri);
        }
    }


    private void OnFileManagerFileCollectingCompleted(object? sender, EventArgs e)
    {
        var fileCollectingCompletedEventArgs = (LLMFileManager.FileCollectingCompletedEventArgs)e;

        if (!fileCollectingCompletedEventArgs.Success && fileCollectingCompletedEventArgs.Exception != null)
        {
            _appSettingsService.ModelStorageDirectory = LMKitDefaultSettings.DefaultModelStorageDirectory;

            _snackbarService!.Show("Error with your model folder", fileCollectingCompletedEventArgs.Exception.Message + $"<br/><b>Model folder has been reset to the default one</b>");
        }
    }

    private void OnModelLoadingFailed(object? sender, EventArgs e)
    {
        var modelLoadingFailedEventArgs = (LMKitService.ModelLoadingFailedEventArgs)e;

        _snackbarService!.Show("Error loading model", modelLoadingFailedEventArgs.Exception.Message!);
    }

    public void SaveAppSettings()
    {
        _settingsViewModel.Save();
    }
}
