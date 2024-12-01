using CommunityToolkit.Mvvm.ComponentModel;
using LMKit.TextGeneration.Chat;
using LMKit.Maestro.Models;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Services;
using static LMKit.TextGeneration.TextGenerationResult;

namespace LMKit.Maestro.ViewModels;

public partial class MessageViewModel : ViewModelBase
{
    public ConversationViewModel ParentConversation { get; }

    private ChatHistory.Message? _lmKitMessage;

    public ChatHistory.Message? LMKitMessage
    {
        get => _lmKitMessage;
        set
        {
            _lmKitMessage = value;

            if (_lmKitMessage != null)
            {
                MessageInProgress = !_lmKitMessage.IsProcessed;
                Sender = AuthorRoleToMessageSender(_lmKitMessage.AuthorRole);
                Text = _lmKitMessage.Content;
                TerminationReason = _lmKitMessage.TerminationReason;
                GeneratedTokens = _lmKitMessage.GeneratedTokens;
                PreviousContent = _lmKitMessage.PreviousContent;
                _lmKitMessage.PropertyChanged += OnMessagePropertyChanged;
            }

            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    private MessageSender _sender;

    [ObservableProperty]
    private string _text;

    [ObservableProperty]
    private bool _messageInProgress;

    [ObservableProperty]
    private LMKitTextGenerationStatus _status;

    [ObservableProperty]
    private bool _isHovered;

    [ObservableProperty]
    private StopReason _terminationReason;

    [ObservableProperty]
    private double _generatedTokens;

    [ObservableProperty]
    private IReadOnlyList<string>? _previousContent;

    public event EventHandler? MessageContentUpdated;

    public MessageViewModel(ConversationViewModel parentConversation, ChatHistory.Message message)
    {
        ParentConversation = parentConversation;
        Text = message.Content;
        LMKitMessage = message;
    }

    [RelayCommand]
    private void ToggleHoveredState()
    {
        IsHovered = !IsHovered;
    }

    private void OnMessagePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ChatHistory.Message.IsProcessed))
        {
            MessageInProgress = !LMKitMessage!.IsProcessed;
        }
        else if (e.PropertyName == nameof(ChatHistory.Message.AuthorRole))
        {
            Sender = AuthorRoleToMessageSender(LMKitMessage!.AuthorRole);
        }
        else if (e.PropertyName == nameof(ChatHistory.Message.Content))
        {
            Text = LMKitMessage!.Content;
            MessageContentUpdated?.Invoke(this, EventArgs.Empty);
        }
        else if (e.PropertyName == nameof(ChatHistory.Message.GeneratedTokens))
        {
            GeneratedTokens = LMKitMessage!.GeneratedTokens;
        }
        else if (e.PropertyName == nameof(ChatHistory.Message.TerminationReason))
        {
            TerminationReason = LMKitMessage!.TerminationReason;
        }
        else if (e.PropertyName == nameof(ChatHistory.Message.PreviousContent))
        {
            PreviousContent = LMKitMessage!.PreviousContent;
        }
    }

    private static MessageSender AuthorRoleToMessageSender(AuthorRole authorRole)
    {
        switch (authorRole)
        {
            case AuthorRole.User:
                return MessageSender.User;

            case AuthorRole.Assistant:
                return MessageSender.Assistant;

            default:
                return MessageSender.Undefined;
        }
    }
}