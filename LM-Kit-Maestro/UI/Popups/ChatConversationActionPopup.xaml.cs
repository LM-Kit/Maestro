using LMKitMaestro.ViewModels;
using Mopups.Interfaces;

namespace LMKitMaestro.UI;

public partial class ChatConversationActionPopup : PopupBase
{
    private ChatConversationActionPopupViewModel _chatConversationActionPopupViewModel;

    public ChatConversationActionPopup(IPopupNavigation popupNavigation, ChatConversationActionPopupViewModel chatConversationActionPopupViewModel) : base(popupNavigation)
    {
        InitializeComponent();

        BindingContext = chatConversationActionPopupViewModel;
        _chatConversationActionPopupViewModel = chatConversationActionPopupViewModel;
    }

    private async void OnSelectClicked(object sender, EventArgs e)
    {
        await SetResult(ChatConversationAction.Select);
    }

    private async void OnRenameClicked(object sender, EventArgs e)
    {
        await SetResult(ChatConversationAction.Rename);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        await SetResult(ChatConversationAction.Delete);
    }

    async void OnCloseButtonClicked(object? sender, EventArgs e) => await SetResult(null);

    private void OnPopupContentSizeChanged(object sender, EventArgs e)
    {
        var popupBottom = popupContentGrid.Height + _chatConversationActionPopupViewModel.ConversationY;
        var pageBottom = _chatConversationActionPopupViewModel.ConversationListHeight + AppConstants.ChatPageTopBarHeight;

        if (popupBottom > pageBottom)
        {
            // If the popup overlaps, align popup bottom with the bottom of the list item.
            popupContentGrid.TranslationY -= popupContentGrid.Height - _chatConversationActionPopupViewModel.ConversationItemHeight;
            _chatConversationActionPopupViewModel.ConversationY -= popupBottom - pageBottom;
        }
    }

    private async void OnPopupBackgroundTapped(object sender, TappedEventArgs e)
    {
        await SetResult(null, true);
    }
}