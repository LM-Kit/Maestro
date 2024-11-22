using LMKitMaestro.ViewModels;

namespace LMKitMaestro.UI;

public partial class ConversationListItemView : ContentView
{
    public ConversationViewModel? ConversationViewModel { get; private set; }

    private EventHandler? _showMoreClicked;
    private readonly object _showMoreClickedLock = new object();

    public event EventHandler? ShowMoreClicked
    {
        add
        {
            lock (_showMoreClickedLock)
            {
                _showMoreClicked -= value;
                _showMoreClicked += value;
            }
        }
        remove
        {
            lock (_showMoreClickedLock)
            {
                _showMoreClicked -= value;
            }
        }
    }

    public ConversationListItemView()
    {
        InitializeComponent();
    }

    private void ConversationTitleFocused(object sender, FocusEventArgs e)
    {
        conversationTitle.Text = ConversationViewModel!.Title;
        conversationTitle.CursorPosition = conversationTitle.Text.Length;
    }

    private void ConversationTitleUnfocused(object sender, FocusEventArgs e)
    {
        ValidateNewConversationTitle();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is ConversationViewModel conversationViewModel)
        {
            ConversationViewModel = conversationViewModel;
            ConversationViewModel.PropertyChanged += OnConversationViewModelPropertyChanged;
        }
    }

    private void OnConversationViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConversationViewModel.IsRenaming))
        {
            if (ConversationViewModel!.IsRenaming)
            {
                conversationTitle.Focus();
            }
        }
    }

    private void ValidateNewConversationTitle()
    {
        if (!string.IsNullOrWhiteSpace(conversationTitle!.Text))
        {
            ConversationViewModel!.Title = conversationTitle.Text.TrimStart().TrimEnd();
        }

        ConversationViewModel!.IsRenaming = false;
    }

    private void OnHovered(object sender, EventArgs e)
    {
        ConversationViewModel!.IsHovered |= true;
    }

    private void OnHoverExited(object sender, EventArgs e)
    {
        ConversationViewModel!.IsHovered &= false;
    }

    private void OnShowMoreButtonClicked(object sender, EventArgs e)
    {
        var test = _showMoreClicked?.GetInvocationList();

        _showMoreClicked?.Invoke(this, e);
    }
}