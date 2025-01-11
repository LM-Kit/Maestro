using LMKit.Maestro.Models;
using LMKit.Maestro.Services;
using LMKit.Maestro.ViewModels;
using Majorsoft.Blazor.Components.Common.JsInterop.GlobalMouseEvents;
using Microsoft.JSInterop;
using MudBlazor;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LMKit.Maestro.UI.Razor.Components;

public partial class Chat
{
    private const int UIUpdateDelayMilliseconds = 50;

    private ConversationViewModel? _previousConversationViewModel;
    private MessageViewModel? _latestAssistantResponse;
    private bool _hasPendingScrollEnd;
    private bool _ignoreScrollsUntilNextScrollUp;
    private double? _previousScrollTop;
    private bool _shouldAutoScrollEnd;
    private double _scrollTop;

    private bool _isScrolledToEnd = false;
    private bool _isShowingActionPopup;

    public bool IsScrolledToEnd
    {
        get => _isScrolledToEnd;
        set
        {
            _isScrolledToEnd = value;

            if (_isScrolledToEnd &&
                ViewModel.ConversationListViewModel.CurrentConversation != null &&
                ViewModel.ConversationListViewModel.CurrentConversation!.AwaitingResponse)
            {
                // Assistant is currently generating a response, enforce auto-scroll.
                _shouldAutoScrollEnd |= true;
            }

            UpdateUIAsync();
        }
    }

    protected override async void OnInitialized()
    {
        base.OnInitialized();

        if (ViewModel.ConversationListViewModel.CurrentConversation != null)
        {
            OnConversationSet();
        }

        ViewModel.ConversationListViewModel.PropertyChanged += OnConversationListViewModelPropertyChanged;
        ViewModel.ConversationListViewModel.CurrentConversation.LMKitConversation.PropertyChanged += OnLMKitConversationPropertyChanged;

        await ResizeHandler.RegisterPageResizeAsync(Resized);
        await JS.InvokeVoidAsync("initializeScrollHandler", DotNetObjectReference.Create(this));
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (_hasPendingScrollEnd)
        {
            _hasPendingScrollEnd = false;
            await ScrollToEnd();
        }
    }

    private bool _refreshScheduled = false;

    private Task UpdateUIAsync(bool forceRedraw = false)
    {
        if (forceRedraw)
        {
            return InvokeAsync(() => StateHasChanged());
        }
        else
        {
            if (!_refreshScheduled)
            {
                _refreshScheduled = true;

                return InvokeAsync(async () =>
                {
                    await Task.Delay(UIUpdateDelayMilliseconds);
                    StateHasChanged();
                    _refreshScheduled = false;
                });
            }
        }

        return Task.CompletedTask;
    }

    private void OnConversationListViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConversationListViewModel.CurrentConversation))
        {
            OnConversationSet();
        }
    }

    private void OnLMKitConversationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LMKitService.Conversation.ContextRemainingSpace))
        {
            UpdateUIAsync();
        }
        else if (e.PropertyName == nameof(LMKitService.Conversation.InTextCompletion))
        {
            UpdateUIAsync(forceRedraw: true);
        }
    }

    private void OnLatestAssistantMessagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MessageViewModel.Content))
        {
            OnLatestAssistantResponseProgressed();
        }
        else if (e.PropertyName == nameof(MessageViewModel.MessageInProgress))
        {
            if (sender == _latestAssistantResponse && !_latestAssistantResponse!.MessageInProgress && _shouldAutoScrollEnd)
            {
                Task.Run(async () =>
                {
                    // Note: Adding delay to sync with current UI behavior
                    // (show latest assistant response footer for 3 sec).
                    await Task.Delay(50);
                    await ScrollToEnd();
                });
            }
        }
    }

    private void OnConversationSet()
    {
        if (_previousConversationViewModel != null)
        {
            _previousConversationViewModel.Messages.CollectionChanged -= OnConversationMessagesCollectionChanged;
            _previousConversationViewModel.TextGenerationCompleted -= OnTextGenerationCompleted;
        }

        _previousConversationViewModel = ViewModel.ConversationListViewModel.CurrentConversation;

        if (ViewModel.ConversationListViewModel.CurrentConversation != null)
        {
            ViewModel.ConversationListViewModel.CurrentConversation.Messages.CollectionChanged += OnConversationMessagesCollectionChanged;
            ViewModel.ConversationListViewModel.CurrentConversation.TextGenerationCompleted += OnTextGenerationCompleted;

            _previousScrollTop = null;
            _ignoreScrollsUntilNextScrollUp = true;
            IsScrolledToEnd = true;

            // Awaiting for the component to be rendered before scrolling to bottom.
            _hasPendingScrollEnd = true;
        }
    }

    private async void OnConversationMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        await UpdateUIAsync(forceRedraw: true);

        await ScrollToEnd();

        var latestMessage = ViewModel.ConversationListViewModel.CurrentConversation!.Messages.LastOrDefault();

        if (latestMessage != null && latestMessage.Sender == MessageSender.Assistant && latestMessage.MessageInProgress)
        {
            _latestAssistantResponse = latestMessage;
            _shouldAutoScrollEnd = true;
            latestMessage.PropertyChanged += OnLatestAssistantMessagePropertyChanged;
        }
    }

    private void OnTextGenerationCompleted(object? sender, ConversationViewModel.TextGenerationCompletedEventArgs e)
    {
        if (e.Exception != null &&
            e.Status == LMKitRequestStatus.GenericError)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            Snackbar.Add($"Text generation failed unexpectedly:\n{e.Exception.Message}", Severity.Error);
        }

        UpdateUIAsync();
    }

    private async void OnLatestAssistantResponseProgressed()
    {
        if (_shouldAutoScrollEnd)
        {
            await ScrollToEnd();
        }
    }

    private async Task Resized(ResizeEventArgs args)
    {
        if (IsScrolledToEnd)
        {
            await ScrollToEnd();
        }
        else
        {
            await CheckIsScrolledToEnd();
        }
    }

    private async Task ScrollToEnd(bool smooth = false)
    {
        IsScrolledToEnd = true;
        _ignoreScrollsUntilNextScrollUp = true;

        await JS.InvokeVoidAsync("scrollToEnd", smooth);
    }

    private async Task CheckIsScrolledToEnd()
    {
        var viewHeight = await JS.InvokeAsync<double>("getConversationViewHeight");
        var chatContentHeight = await JS.InvokeAsync<double>("getScrollHeight");

        var value = chatContentHeight - viewHeight - _scrollTop;
        IsScrolledToEnd = Math.Abs(chatContentHeight - viewHeight - _scrollTop) < 5 || chatContentHeight <= viewHeight;
    }

    private async Task OnScrollToEndButtonClicked()
    {
        await ScrollToEnd(true);
    }

    private void OnConversationItemSelected(ConversationViewModel conversationViewModel)
    {
        if (conversationViewModel != ViewModel.ConversationListViewModel.CurrentConversation)
        {
            ViewModel.ConversationListViewModel.CurrentConversation = conversationViewModel;
        }
    }

    private void OnShowMoreClicked(ConversationViewModel conversationViewModel)
    {
        if (_isShowingActionPopup)
        {
            return;
        }

        _isShowingActionPopup = true;

        ChatConversationActionPopupViewModel chatConversationActionPopupViewModel = new ChatConversationActionPopupViewModel()
        {
            ConversationX = 400,
            //ConversationY = conversationItem.Y - collectionView.ScrollY + conversationItem.Height + UIConstants.TabBarHeight + UIConstants.PageTopBarHeight,
            //ConversationItemHeight = conversationItem.Height,
            //ConversationListHeight = Height
        };

        var popup = new ChatConversationActionPopup(ViewModel.PopupNavigation, chatConversationActionPopupViewModel)
        {
            Animation = new Mopups.Animations.FadeAnimation()
            {
                DurationIn = 1,
                DurationOut = 1,
                EasingIn = Easing.Linear,
                EasingOut = Easing.Linear
            }
        };

        Task.Run(async () =>
        {
            conversationViewModel!.IsShowingActionPopup = true;
            await ViewModel.PopupNavigation.PushAsync(popup);

            var result = await popup.PopupTask;

            if (result != null && result.Value is ChatConversationAction chatConversationAction)
            {
                switch (chatConversationAction)
                {
                    case ChatConversationAction.Select:
                        ViewModel.ConversationListViewModel.CurrentConversation = conversationViewModel!;
                        break;

                    case ChatConversationAction.Rename:
                        conversationViewModel!.IsRenaming = true;
                        break;

                    case ChatConversationAction.Delete:
                        await ViewModel.ConversationListViewModel.DeleteConversation(conversationViewModel!);
                        break;
                }
            }
            conversationViewModel!.IsShowingActionPopup = false;
        });
    }

    private int CalculateUsagePercentage(int used, int total)
    {
        if (total == 0) return 0;
        return (int)(100 * used / total);
    }


    [JSInvokable]
    public async Task OnChatScrolled(double scrollTop)
    {
        _scrollTop = scrollTop;

        bool shouldCheckIsScrolledToEnd = true;
        bool? isScrollUp = null;

        if (_previousScrollTop != null)
        {
            isScrollUp = scrollTop < _previousScrollTop;

            if (isScrollUp.Value && _shouldAutoScrollEnd)
            {
                _shouldAutoScrollEnd = false;
            }
        }

        if (_ignoreScrollsUntilNextScrollUp)
        {
            if (isScrollUp == null || !isScrollUp.Value)
            {
                shouldCheckIsScrolledToEnd = false;
            }
            else
            {
                _ignoreScrollsUntilNextScrollUp = false;
            }
        }

        if (shouldCheckIsScrolledToEnd)
        {
            await CheckIsScrolledToEnd();
        }

        _previousScrollTop = _scrollTop;
    }
}
