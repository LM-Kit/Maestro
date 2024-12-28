using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Models;
using LMKit.Maestro.Services;
using LMKit.TextGeneration.Chat;
using static LMKit.TextGeneration.TextGenerationResult;

namespace LMKit.Maestro.ViewModels;

public partial class MessageViewModel : ViewModelBase
{
    public ConversationViewModel ParentConversation { get; }

    public ChatHistory.Message? LMKitMessage { get; }

    [ObservableProperty]
    private MessageSender _sender;

    [ObservableProperty]
    private string _content;

    [ObservableProperty]
    private bool _messageInProgress;

    [ObservableProperty]
    private LMKitRequestStatus _status;

    [ObservableProperty]
    private bool _isHovered;

    [ObservableProperty]
    private bool _isLastAssistantMessage;

    public event EventHandler? OnRegeneratedResponse;

    public StopReason GetTerminationReason(int messageIndex)
    {
        return GetMessageByIndex(messageIndex).TerminationReason;
    }

    public int GetGeneratedTokens(int messageIndex)
    {
        return GetMessageByIndex(messageIndex).GeneratedTokens;
    }

    public string GetContent(int messageIndex)
    {
        return GetMessageByIndex(messageIndex).Content;
    }

    public int GetResponseCount()
    {
        if (LMKitMessage != null)
        {
            if (LMKitMessage.PreviousContent != null && LMKitMessage.PreviousContent.Count > 0)
            {
                return LMKitMessage.PreviousContent.Count + 1;
            }

            return 1;
        }

        throw new InvalidOperationException();
    }

    private ChatHistory.Message GetMessageByIndex(int index)
    {
        if (LMKitMessage != null)
        {
            if (LMKitMessage.PreviousContent != null && LMKitMessage.PreviousContent.Count > 0)
            {
                if (index == LMKitMessage.PreviousContent.Count)
                {
                    return LMKitMessage;
                }
                else
                {
                    return LMKitMessage.PreviousContent[index];
                }
            }
            else
            {
                return LMKitMessage;
            }
        }

        throw new InvalidOperationException();
    }

    public MessageViewModel(ConversationViewModel parentConversation, ChatHistory.Message message)
    {
        ParentConversation = parentConversation;
        LMKitMessage = message;
        MessageInProgress = !LMKitMessage.IsProcessed;
        Sender = AuthorRoleToMessageSender(LMKitMessage.AuthorRole);
        Content = LMKitMessage.Content;
        LMKitMessage.PropertyChanged += OnMessagePropertyChanged;
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
            Content = LMKitMessage!.Content;
        }
        else if (e.PropertyName == nameof(ChatHistory.Message.PreviousContent))
        {
            OnRegeneratedResponse?.Invoke(this, EventArgs.Empty);
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