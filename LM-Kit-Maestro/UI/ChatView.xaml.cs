using LMKitMaestro.ViewModels;
using LMKit.TextGeneration;

namespace LMKitMaestro.UI;

public partial class ChatView : ContentView
{
    private ConversationViewModel? _previouslySelectedConversation;

    public static readonly BindableProperty ChatEntryIsFocusedProperty = BindableProperty.Create(nameof(ChatEntryIsFocused), typeof(bool), typeof(ChatView));
    public bool ChatEntryIsFocused
    {
        get => (bool)GetValue(ChatEntryIsFocusedProperty);
        private set => SetValue(ChatEntryIsFocusedProperty, value);
    }

    public static readonly BindableProperty ShowLatestCompletionResultProperty = BindableProperty.Create(nameof(ShowLatestCompletionResult), typeof(bool), typeof(ChatView));
    public bool ShowLatestCompletionResult
    {
        get => (bool)GetValue(ShowLatestCompletionResultProperty);
        private set => SetValue(ShowLatestCompletionResultProperty, value);
    }

    public static readonly BindableProperty? LatestStopReasonProperty = BindableProperty.Create(nameof(LatestStopReason), typeof(TextGenerationResult.StopReason?), typeof(ChatView));
    public TextGenerationResult.StopReason? LatestStopReason
    {
        get => (TextGenerationResult.StopReason?)GetValue(LatestStopReasonProperty);
        private set => SetValue(LatestStopReasonProperty, value);
    }

    private ConversationViewModel? _conversationViewModel;

    public ChatView()
    {
        InitializeComponent();
    }


    protected async override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is ConversationViewModel conversationViewModel)
        {
            _conversationViewModel = conversationViewModel;

            conversationViewModel.TextGenerationCompleted += OnTextGenerationCompleted;

            if (ShowLatestCompletionResult)
            {
                ShowLatestCompletionResult = false;
            }

            LatestStopReason = null;

            if (_previouslySelectedConversation != null)
            {
                _previouslySelectedConversation.TextGenerationCompleted -= OnTextGenerationCompleted;
            }

            _previouslySelectedConversation = conversationViewModel;

            await ForceFocus();
        }
    }

    private async Task ForceFocus()
    {
        //if (!chatBoxEditor.IsFocused)
        {
            do
            {
                await Task.Delay(100);
            }
            while (!chatBoxEditor.Focus());
        }
    }

    private void OnEntryKeyReleased(object sender, EventArgs e)
    {
        if (_conversationViewModel != null && !string.IsNullOrWhiteSpace(_conversationViewModel.InputText) && !_conversationViewModel.AwaitingResponse)
        {
            _conversationViewModel.Send();
        }
    }

    private void OnEntryBorderFocused(object sender, FocusEventArgs e)
    {
        ChatEntryIsFocused = true;
    }

    private void OnEntryBorderUnfocused(object sender, FocusEventArgs e)
    {
        ChatEntryIsFocused = false;
    }

    private void OnTextGenerationCompleted(object? sender, EventArgs e)
    {
        var textGenerationCompletedEventArgs = (ConversationViewModel.TextGenerationCompletedEventArgs)e;

        if (sender == BindingContext)
        {
            var _ = Task.Run(async () =>
            {
                ShowLatestCompletionResult = true;
                LatestStopReason = textGenerationCompletedEventArgs.StopReason;
                await Task.Delay(3000);
                LatestStopReason = null;
                ShowLatestCompletionResult = false;
            });
        }
    }
}
