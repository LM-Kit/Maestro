using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using LMKit.Maestro.Services;
using LMKit.Maestro.Helpers;

namespace LMKit.Maestro.ViewModels;

public partial class AppShellViewModel : ViewModelBase
{
    private readonly IPopupService _popupService;
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
    List<MaestroTabViewModel> _tabs = new List<MaestroTabViewModel>();

    [ObservableProperty]
    MaestroTabViewModel _chatTab = new MaestroTabViewModel("Chat", "ChatPage");

    [ObservableProperty]
    MaestroTabViewModel _modelsTab = new MaestroTabViewModel("Models", "ModelsPage");

    [ObservableProperty]
    MaestroTabViewModel _assistantsTab = new MaestroTabViewModel("Assistants", "AssistantsPage");

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

    public AppShellViewModel(IPopupService popupService, INavigationService navigationService, ILogger<AppShellViewModel> logger,
        ConversationListViewModel conversationListViewModel, ModelListViewModel modelListViewModel,
        SettingsViewModel settingsViewModel, LMKitService lmKitService,
        ILLMFileManager llmFileManager, IAppSettingsService appSettingsService)
    {
        _popupService = popupService;
        _logger = logger;
        _conversationListViewModel = conversationListViewModel;
        _modelListViewModel = modelListViewModel;
        _settingsViewModel = settingsViewModel;
        _lmKitService = lmKitService;
        _llmFileManager = llmFileManager;
        _appSettingsService = appSettingsService;

        Tabs.Add(AssistantsTab);
        Tabs.Add(ChatTab);
        Tabs.Add(ModelsTab);

        CurrentTab = AssistantsTab;
    }

    public async Task Init()
    {
        _settingsViewModel.Init();

        await _conversationListViewModel.LoadConversationLogs();

        _llmFileManager.FileCollectingCompleted += OnFileManagerFileCollectingCompleted;
        _llmFileManager.Initialize();

        _lmKitService.ModelLoadingFailed += OnModelLoadingFailed;

        if (_appSettingsService.LastLoadedModel != null)
        {
            _ = Task.Run(TryLoadLastUsedModel); //Loading model in the background to avoid blocking UI initialization.
        }

        // todo: we should ensure UI is loaded before starting loading a model with this call.
        _modelListViewModel.Initialize();

        AppIsInitialized = true;
    }

    private void TryLoadLastUsedModel()
    {
        if (FileHelpers.TryCreateFileUri(_appSettingsService.LastLoadedModel!, out Uri? fileUri) &&
            File.Exists(_appSettingsService.LastLoadedModel)/* &&
            FileHelpers.GetModelInfoFromFileUri(fileUri!, _appSettingsService.ModelsFolderPath,
            out string publisher, out string repository, out string fileName)*/)
        {
            _lmKitService.LoadModel(fileUri!);
        }
        else
        {
            _appSettingsService.LastLoadedModel = null;
        }
    }

    private void OnFileManagerFileCollectingCompleted(object? sender, EventArgs e)
    {
        var fileCollectingCompletedEventArgs = (LLMFileManager.FileCollectingCompletedEventArgs)e;

        if (!fileCollectingCompletedEventArgs.Success && fileCollectingCompletedEventArgs.Exception != null)
        {
            _appSettingsService.ModelsFolderPath = LMKitDefaultSettings.DefaultModelsFolderPath;

            if (Microsoft.Maui.ApplicationModel.MainThread.IsMainThread)
            {
                _popupService!.DisplayAlert("Error with your model folder",
                    $"Model files failed to be collected from the input folder:\n{fileCollectingCompletedEventArgs.Exception.Message!}\n\nYour model folder has been reset to the default one.",
                    "OK");
            }
            else
            {
                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                _popupService!.DisplayAlert("Error with your model folder",
                    $"Model files failed to be collected from the input folder:\n{fileCollectingCompletedEventArgs.Exception.Message!}\n\nYour model folder has been reset to the default one.",
                    "OK"));
            }
        }
    }

    private void OnModelLoadingFailed(object? sender, EventArgs e)
    {
        var modelLoadingFailedEventArgs = (LMKitService.ModelLoadingFailedEventArgs)e;

        if (Microsoft.Maui.ApplicationModel.MainThread.IsMainThread)
        {
            _popupService.DisplayAlert("Error loading model", $"The model failed to be loaded: {modelLoadingFailedEventArgs.Exception.Message}", "OK");
        }
        else
        {
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            _popupService.DisplayAlert("Error loading model", $"The model failed to be loaded: {modelLoadingFailedEventArgs.Exception.Message}", "OK"));
        }
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
