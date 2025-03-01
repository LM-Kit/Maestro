using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Services;
using Microsoft.Extensions.Logging;

namespace LMKit.Maestro.ViewModels;

public partial class AppShellViewModel : ViewModelBase
{
    private readonly ISnackbarService _snackbarService;
    private readonly ILogger<AppShellViewModel> _logger;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly ModelListViewModel _modelListViewModel;
    private readonly ConversationListViewModel _conversationListViewModel;
    private readonly LMKitService _lmKitService;
    private readonly ILLMFileManager _llmFileManager;
    private readonly IAppSettingsService _appSettingsService;

    [ObservableProperty]
    private bool _appIsInitialized = false;

    [ObservableProperty]
    List<MaestroTabViewModel> _tabs = [];

    [ObservableProperty]
    MaestroTabViewModel _chatTab = new MaestroTabViewModel("Chat", "ChatPage");

    [ObservableProperty]
    MaestroTabViewModel _modelsTab = new MaestroTabViewModel("Models", "ModelsPage");

    // [ObservableProperty]
    // MaestroTabViewModel _assistantsTab = new MaestroTabViewModel("Assistants", "AssistantsPage");

    private MaestroTabViewModel? _currentTab;
    public MaestroTabViewModel CurrentTab
    {
        get => _currentTab!;
        set
        {
            if (_currentTab != null)
            {
                _currentTab.IsSelected = false;
            }

            _currentTab = value;
            _currentTab.IsSelected = true;
        }
    }

    public AppShellViewModel(ISnackbarService snackbarService, INavigationService navigationService, ILogger<AppShellViewModel> logger,
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

        // Tabs.Add(AssistantsTab);
        Tabs.Add(ChatTab);
        Tabs.Add(ModelsTab);

        CurrentTab = ChatTab;
    }

    public async Task Init()
    {
        _settingsViewModel.Init();

        await _conversationListViewModel.LoadConversationLogs();

        _llmFileManager.FileCollectingCompleted += OnFileManagerFileCollectingCompleted;
        _llmFileManager.Initialize();

        _lmKitService.ModelLoadingFailed += OnModelLoadingFailed;

        if (_appSettingsService.LastLoadedModelUri != null)
        {
            _ = Task.Run(TryLoadLastUsedModel); //Loading model in the background to avoid blocking UI initialization.
        }

        // todo: we should ensure UI is loaded before starting loading a model with this call.
        _modelListViewModel.Initialize();

        AppIsInitialized = true;
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

            _snackbarService!.Show("Error with your model folder", fileCollectingCompletedEventArgs.Exception.Message!);
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

    [RelayCommand]
    private async Task Navigate(MaestroTabViewModel tab)
    {
        if (!tab.IsSelected)
        {
            await Shell.Current.GoToAsync($"//{tab.Route}", true);
        }
    }
}
