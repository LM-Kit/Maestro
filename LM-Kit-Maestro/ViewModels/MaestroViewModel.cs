using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Services;
using Microsoft.Extensions.Logging;

namespace LMKit.Maestro.ViewModels;

public partial class MaestroViewModel : ViewModelBase
{
    private readonly ISnackbarService _snackbarService;
    private readonly ILogger<MaestroViewModel> _logger;
    private readonly ChatSettingsViewModel _chatSettingsViewModel;
    private readonly ModelListViewModel _modelListViewModel;
    private readonly ConversationListViewModel _conversationListViewModel;
    private readonly LMKitService _lmKitService;
    private readonly ILLMFileManager _llmFileManager;
    private readonly IAppSettingsService _appSettingsService;

    public MaestroViewModel(ISnackbarService snackbarService, ILogger<MaestroViewModel> logger,
        ConversationListViewModel conversationListViewModel, ModelListViewModel modelListViewModel,
        ChatSettingsViewModel chatSettingsViewModel, LMKitService lmKitService,
        ILLMFileManager llmFileManager, IAppSettingsService appSettingsService)
    {
        _snackbarService = snackbarService;
        _logger = logger;
        _conversationListViewModel = conversationListViewModel;
        _modelListViewModel = modelListViewModel;
        _chatSettingsViewModel = chatSettingsViewModel;
        _lmKitService = lmKitService;
        _llmFileManager = llmFileManager;
        _appSettingsService = appSettingsService;
    }

    public async Task Init()
    {
        _chatSettingsViewModel.Init();
        
        await _conversationListViewModel.LoadConversationLogs();

        EnsureModelDirectoryExists();

        _llmFileManager.FileCollectingCompleted += OnFileManagerFileCollectingCompleted;
        _lmKitService.ModelLoadingFailed += OnModelLoadingFailed;

        if (_appSettingsService.LastLoadedModelUri != null)
        {
            //Loading model in the background to avoid blocking UI initialization.
            _ = Task.Run(TryLoadLastUsedModel);
        }

        _modelListViewModel.Initialize();
    }

    private void EnsureModelDirectoryExists()
    {
        if (!Directory.Exists(_modelListViewModel.ModelsSettings.ModelsDirectory))
        {
            if (!Directory.Exists(LMKitDefaultSettings.DefaultModelStorageDirectory))
            {
                if (File.Exists(LMKitDefaultSettings.DefaultModelStorageDirectory))
                {
                    File.Delete(LMKitDefaultSettings.DefaultModelStorageDirectory);
                }

                Directory.CreateDirectory(LMKitDefaultSettings.DefaultModelStorageDirectory);

                _modelListViewModel.ModelsSettings.ModelsDirectory = LMKitDefaultSettings.DefaultModelStorageDirectory;
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
           _modelListViewModel.ModelsSettings.ModelsDirectory = LMKitDefaultSettings.DefaultModelStorageDirectory;

            _snackbarService!.Show("Error with your model folder", fileCollectingCompletedEventArgs.Exception.Message + $"<br/><b>Model folder has been reset to the default one</b>");
        }
    }

    private void OnModelLoadingFailed(object? sender, EventArgs e)
    {
        // Don't show error if user cancelled the download
        if (_lmKitService.WasLoadingCancelled)
        {
            return;
        }
        
        var modelLoadingFailedEventArgs = (LMKitService.ModelLoadingFailedEventArgs)e;

        _snackbarService!.Show("Error loading model", modelLoadingFailedEventArgs.Exception.Message!);
    }

    public void SaveAppSettings()
    {
        _chatSettingsViewModel.Save();
    }
}
