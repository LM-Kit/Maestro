using CommunityToolkit.Mvvm.ComponentModel;

namespace LMKit.Maestro.ViewModels
{
    public partial class ChatConversationActionPopupViewModel : ViewModelBase
    {
        [ObservableProperty]
        double _conversationX;

        [ObservableProperty]
        double _conversationY;

        [ObservableProperty]
        double _conversationListHeight;

        [ObservableProperty]
        double _conversationItemHeight;

        public void Load(double x, double y, double conversationListHeight, double conversationItemHeight)
        {
            ConversationX = x;
            ConversationY = y;
            ConversationListHeight = conversationListHeight;
            ConversationItemHeight = conversationItemHeight;
        }
    }
}

public enum ChatConversationAction
{
    Select,
    Rename,
    Delete
}