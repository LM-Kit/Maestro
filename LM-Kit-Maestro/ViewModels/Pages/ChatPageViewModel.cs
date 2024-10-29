using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKitMaestro.Data;
using Microsoft.Extensions.Logging;
using LMKitMaestro.Helpers;
using LMKitMaestro.Services;
using Mopups.Interfaces;

namespace LMKitMaestro.ViewModels;

public partial class ChatPageViewModel : PageViewModelBase
{
    private readonly ILogger<ChatPageViewModel> _logger;
    private readonly ILMKitMaestroDatabase _database;
    private readonly ILLMFileManager _llmFileManager;

    [ObservableProperty]
    private bool _chatsSidebarIsToggled;

    [ObservableProperty]
    private bool _settingsSidebarIsToggled;

    [ObservableProperty]
    private double _loadingProgress;

    [ObservableProperty]
    private SettingsViewModel _settingsViewModel;

    [ObservableProperty]
    private bool _modelLoadingIsFinishingUp;

    public LMKitService LmKitService { get; }
    public ConversationListViewModel ConversationListViewModel { get; }
    public ModelListViewModel ModelListViewModel { get; }

    private ModelInfoViewModel? _selectedModel;
    public ModelInfoViewModel? SelectedModel
    {
        get => _selectedModel;
        set
        {
            if (value != _selectedModel)
            {
                _selectedModel = value;
                OnPropertyChanged();

                if (_selectedModel != null && _selectedModel.ModelInfo.FileUri != LmKitService.LMKitConfig.LoadedModelUri)
                {
                    LoadModel(_selectedModel.ModelInfo.FileUri);
                }
            }
        }
    }

    public ChatPageViewModel(INavigationService navigationService, IPopupService popupService, IPopupNavigation popupNavigation,
        ConversationListViewModel conversationListViewModel, ModelListViewModel modelListViewModel,
        ILogger<ChatPageViewModel> logger, ILMKitMaestroDatabase database,
        LMKitService lmKitService, ILLMFileManager llmFileManager, SettingsViewModel settingsViewModel) : base(navigationService, popupService, popupNavigation)
    {
        _logger = logger;
        ConversationListViewModel = conversationListViewModel;
        ModelListViewModel = modelListViewModel;
        _database = database;
        _llmFileManager = llmFileManager;

        LmKitService = lmKitService;
        SettingsViewModel = settingsViewModel;
        LmKitService.ModelLoadingProgressed += OnModelLoadingProgressed;
        LmKitService.ModelLoadingFailed += OnModelLoadingFailed;
        LmKitService.ModelLoadingCompleted += OnModelLoadingCompleted;

        ConversationListViewModel.Conversations.CollectionChanged += OnConversationListChanged;

    }

    public void Initialize()
    {
        if (LmKitService.LMKitConfig.LoadedModelUri != null)
        {
            SelectedModel = LMKitMaestroHelpers.TryGetExistingModelInfoViewModel(ModelListViewModel.UserModels, LmKitService.LMKitConfig.LoadedModelUri);
        }

        //if (LmKitService.ModelLoadingState != ModelLoadingState.Loaded)
        //{
        //    LmKitService.ModelLoadingCompleted += OnModelLoadingCompleted;
        //}
    }

    [RelayCommand]
    public void ToggleChatsSidebar()
    {
        ChatsSidebarIsToggled = !ChatsSidebarIsToggled;
    }

    [RelayCommand]
    public void ToggleSettingsSidebar()
    {
        SettingsSidebarIsToggled = !SettingsSidebarIsToggled;
    }

    [RelayCommand]
    public void StartNewConversation()
    {
        ConversationListViewModel.AddNewConversation();
    }

    [RelayCommand]
    public async Task DeleteConversation(ConversationViewModel conversationViewModel)
    {
        await ConversationListViewModel.DeleteConversation(conversationViewModel);

        if (ConversationListViewModel.Conversations.Count == 0)
        {
            StartNewConversation();
        }
        else if (conversationViewModel == ConversationListViewModel.CurrentConversation)
        {
            ConversationListViewModel.CurrentConversation = ConversationListViewModel.Conversations.First();
        }
    }

    [RelayCommand]
    public void EjectModel()
    {
        if (SelectedModel != null)
        {
            LmKitService.UnloadModel();
            SelectedModel = null;
        }
    }

    [RelayCommand]
    public void LoadModel(Uri fileUri)
    {
        ModelInfo? modelInfo = null;

        foreach (var model in ModelListViewModel.UserModels)
        {
            if (model.ModelInfo.FileUri == fileUri)
            {
                modelInfo = model.ModelInfo;
                break;
            }
        }

        if (modelInfo != null)
        {
            LmKitService.LoadModel(fileUri);
        }
        else
        {
            PopupService.DisplayAlert("Model not found",
                $"This model was not found in your model folder.\nMake sure the path points to your current model folder and that the file exists on your disk: {fileUri.LocalPath}",
                "OK");
        }
    }

    private void InitializeCurrentConversation()
    {
        if (ConversationListViewModel.Conversations.Count == 0)
        {
            StartNewConversation();
        }
    }

    private void OnModelLoadingCompleted(object? sender, EventArgs e)
    {
        SelectedModel = LMKitMaestroHelpers.TryGetExistingModelInfoViewModel(ModelListViewModel.UserModels, LmKitService.LMKitConfig.LoadedModelUri!);
        LoadingProgress = 0;
        ModelLoadingIsFinishingUp = false;
    }

    private void OnModelLoadingProgressed(object? sender, EventArgs e)
    {
        var loadingEventArgs = (LMKitService.ModelLoadingProgressedEventArgs)e;

        LoadingProgress = loadingEventArgs.Progress;
        ModelLoadingIsFinishingUp = LoadingProgress == 1;
    }

    private void OnModelLoadingFailed(object? sender, EventArgs e)
    {
        SelectedModel = null;
    }

    private void OnConversationListChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && ConversationListViewModel.Conversations.Count == 1)
        {
            ConversationListViewModel.CurrentConversation = ConversationListViewModel.Conversations[0];
        }
    }
}