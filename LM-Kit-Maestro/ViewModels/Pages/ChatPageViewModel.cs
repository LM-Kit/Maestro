using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Services;
namespace LMKit.Maestro.ViewModels;

public partial class ChatPageViewModel : ViewModelBase
{
    public ChatSettingsViewModel ChatSettingsViewModel { get; }

    public LMKitService LmKitService { get; }
    public ConversationListViewModel ConversationListViewModel { get; }
    public ModelListViewModel ModelListViewModel { get; }

    public ChatPageViewModel(ConversationListViewModel conversationListViewModel, ModelListViewModel modelListViewModel,
        LMKitService lmKitService, ChatSettingsViewModel chatSettingsViewModel)
    {
        ConversationListViewModel = conversationListViewModel;
        ModelListViewModel = modelListViewModel;
        LmKitService = lmKitService;
        ChatSettingsViewModel = chatSettingsViewModel;
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
    }
}