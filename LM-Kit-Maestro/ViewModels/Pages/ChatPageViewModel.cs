using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Data;
using LMKit.Maestro.Services;
using Microsoft.Extensions.Logging;
namespace LMKit.Maestro.ViewModels;

public partial class ChatPageViewModel : ViewModelBase
{
    private readonly ILogger<ChatPageViewModel> _logger;
    private readonly IMaestroDatabase _database;
    private readonly ILLMFileManager _llmFileManager;

    [ObservableProperty]
    private bool _chatsSidebarIsToggled;

    [ObservableProperty]
    private bool _settingsSidebarIsToggled;

    [ObservableProperty]
    private SettingsViewModel _settingsViewModel;

    public LMKitService LmKitService { get; }
    public ConversationListViewModel ConversationListViewModel { get; }
    public ModelListViewModel ModelListViewModel { get; }

    public ChatPageViewModel(ConversationListViewModel conversationListViewModel, ModelListViewModel modelListViewModel,
        ILogger<ChatPageViewModel> logger, IMaestroDatabase database,
        LMKitService lmKitService, ILLMFileManager llmFileManager, SettingsViewModel settingsViewModel)
    {
        _logger = logger;
        ConversationListViewModel = conversationListViewModel;
        ModelListViewModel = modelListViewModel;
        _database = database;
        _llmFileManager = llmFileManager;
        LmKitService = lmKitService;
        SettingsViewModel = settingsViewModel;

        ConversationListViewModel.Conversations.CollectionChanged += OnConversationListChanged;
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
    private void OnConversationListChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && ConversationListViewModel.Conversations.Count == 1)
        {
            ConversationListViewModel.CurrentConversation = ConversationListViewModel.Conversations[0];
        }
    }
}