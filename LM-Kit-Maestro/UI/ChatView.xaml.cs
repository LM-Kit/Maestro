using LMKit.Maestro.ViewModels;

namespace LMKit.Maestro.UI;

public partial class ChatView : ContentView
{
    private ConversationViewModel? _previouslySelectedConversation;

    public static readonly BindableProperty ChatEntryIsFocusedProperty = BindableProperty.Create(nameof(ChatEntryIsFocused), typeof(bool), typeof(ChatView));
    public bool ChatEntryIsFocused
    {
        get => (bool)GetValue(ChatEntryIsFocusedProperty);
        private set => SetValue(ChatEntryIsFocusedProperty, value);
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
            _conversationViewModel.Submit();
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
}
