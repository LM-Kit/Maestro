using LMKitMaestro.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Mopups.Animations;

namespace LMKitMaestro.UI;

public partial class ChatConversationsView : ContentView
{
    private bool _isShowingActionPopup;
    private ChatPageViewModel? _chatPageViewModel;

    public ChatConversationsView()
    {
        InitializeComponent();

        collectionView.ChildViewAdded += CollectionView_ChildViewAdded;
    }

    private void CollectionView_ChildViewAdded(object? sender, ElementEventArgs e)
    {
        if (e.Element is ConversationListItemView conversationListItemView)
        {
            conversationListItemView.ShowMoreClicked += OnConversationListItemShowMoreClicked;
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is ChatPageViewModel chatPageViewModel)
        {
            _chatPageViewModel = chatPageViewModel;
        }
    }

    [RelayCommand]
    private void ConversationTapped(ConversationViewModel conversationViewModel)
    {
        if (_chatPageViewModel != null)
        {
            if (_chatPageViewModel.ConversationListViewModel.CurrentConversation != conversationViewModel)
            {
                _chatPageViewModel.ConversationListViewModel.CurrentConversation = conversationViewModel;

            }
        }
    }

    private void ConversationListItemViewTapped(object sender, EventArgs e)
    {
        if (_chatPageViewModel != null &&
            sender is ConversationListItemView conversationListItemView &&
            conversationListItemView.BindingContext is ConversationViewModel conversationViewModel)
        {
            if (conversationViewModel != _chatPageViewModel.ConversationListViewModel.CurrentConversation)
            {
                _chatPageViewModel.ConversationListViewModel.CurrentConversation = conversationViewModel;

            }
        }
    }

    private async void OnConversationListItemShowMoreClicked(object? sender, EventArgs e)
    {
        var conversationItem = (ConversationListItemView)sender!;

        if (_isShowingActionPopup)
        {
            return;
        }

        _isShowingActionPopup = true;

        if (_chatPageViewModel != null)
        {
            // CommunityToolkit doesn't allow changing popup overlay yet -> using Mopup instead.
            ChatConversationActionPopupViewModel chatConversationActionPopupViewModel = new ChatConversationActionPopupViewModel()
            {
                ConversationX = collectionView.ScrollX + conversationItem.Width,
                ConversationY = conversationItem.Y - collectionView.ScrollY + conversationItem.Height + AppConstants.ChatPageTopBarHeight,
                ConversationItemHeight = conversationItem.Height,
                ConversationListHeight = Height
            };

            var popup = new ChatConversationActionPopup(_chatPageViewModel.PopupNavigation, chatConversationActionPopupViewModel)
            {
                Animation = new FadeAnimation()
                {
                    DurationIn = 1,
                    DurationOut = 1,
                    EasingIn = Easing.Linear,
                    EasingOut = Easing.Linear
                }
            };

            conversationItem.ConversationViewModel!.IsShowingActionPopup = true;
            await _chatPageViewModel.PopupNavigation.PushAsync(popup);

            var result = await popup.PopupTask;

            if (result != null && result.Value is ChatConversationAction chatConversationAction)
            {
                switch (chatConversationAction)
                {
                    case ChatConversationAction.Select:
                        _chatPageViewModel.ConversationListViewModel.CurrentConversation = conversationItem.ConversationViewModel!;
                        break;

                    case ChatConversationAction.Rename:
                        conversationItem.ConversationViewModel!.IsRenaming = true;
                        break;

                    case ChatConversationAction.Delete:
                        await _chatPageViewModel.ConversationListViewModel.DeleteConversation(conversationItem.ConversationViewModel!);
                        break;
                }
            }

            conversationItem.ConversationViewModel!.IsShowingActionPopup = false;
            _isShowingActionPopup = false;
        }
    }
}