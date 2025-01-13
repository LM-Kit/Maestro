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
    private ConversationViewModel? _conversationShowingMore;
    private MessageViewModel? _latestAssistantResponse;

    private bool _autoScrolling;
    private bool _hasPendingScrollToEnd;
    private bool _ignoreScrollsUntilNextScrollUp;
    private double? _previousScrollTop;
    private double _scrollTop;

    private bool _refreshScheduled;

    private bool _isScrolledToEnd;
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
                _autoScrolling |= true;
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
        
        ViewModel.ConversationListViewModel.ConversationPropertyChanged += OnConversationPropertyChanged;
        ViewModel.ConversationListViewModel.PropertyChanged += OnConversationListViewModelPropertyChanged;

        await ResizeHandler.RegisterPageResizeAsync(Resized);
        await JS.InvokeVoidAsync("initializeScrollHandler", DotNetObjectReference.Create(this));
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (_hasPendingScrollToEnd)
        {
            _hasPendingScrollToEnd = false;
            await ScrollToEnd();
        }
    }


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

    private void OnConversationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConversationViewModel.IsShowingActionPopup))
        {
            UpdateUIAsync();
        }
    }
    private void OnCurrentLMKitConversationPropertyChanged(object? sender, PropertyChangedEventArgs e)
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
            if (sender == _latestAssistantResponse && !_latestAssistantResponse!.MessageInProgress && _autoScrolling)
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
            ViewModel.ConversationListViewModel.CurrentConversation.LMKitConversation.PropertyChanged += OnCurrentLMKitConversationPropertyChanged;

            _previousScrollTop = null;
            _ignoreScrollsUntilNextScrollUp = true;
            IsScrolledToEnd = true;

            // Awaiting for the component to be rendered before scrolling to bottom.
            _hasPendingScrollToEnd = true;
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
            _autoScrolling = true;
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
        if (_autoScrolling)
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

            if (isScrollUp.Value && _autoScrolling)
            {
                _autoScrolling = false;
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

    private void OnConversationItemSelected(ConversationViewModel conversationViewModel)
    {
        if (conversationViewModel != ViewModel.ConversationListViewModel.CurrentConversation)
        {
            ViewModel.ConversationListViewModel.CurrentConversation = conversationViewModel;
        }

        if (_conversationShowingMore != null)
        {
            _conversationShowingMore.IsShowingActionPopup = false;
            _conversationShowingMore = null;
        }
    }

    private void OnConversationItemShowMoreClicked(ConversationViewModel conversationViewModel)
    {
        if (_conversationShowingMore != null)
        {
            _conversationShowingMore.IsShowingActionPopup = false;
            _conversationShowingMore = null;
        }

        _conversationShowingMore = conversationViewModel;
        _conversationShowingMore.IsShowingActionPopup = true;
    }


    private void OnConversationItemDeleteClicked(ConversationViewModel conversationViewModel)
    {
        Task.Run(async () => await ViewModel.ConversationListViewModel.DeleteConversation(conversationViewModel));
    }
}
