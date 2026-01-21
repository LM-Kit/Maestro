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

    private ChatHistory.Message _lmKitMessage;

    public ChatHistory.Message LMKitMessage
    {
        get => _lmKitMessage;
        set
        {
            if (_lmKitMessage != value)
            {
                _lmKitMessage = value;
                _lmKitMessage.PropertyChanged += OnMessagePropertyChanged;
                OnPropertyChanged();
            }
        }
    }

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

    /// <summary>
    /// Attachments associated with this message (images, PDFs, etc.)
    /// </summary>
    public IReadOnlyList<ChatAttachment> Attachments { get; private set; } = Array.Empty<ChatAttachment>();

    /// <summary>
    /// Whether this message has any attachments.
    /// </summary>
    public bool HasAttachments => Attachments != null && Attachments.Count > 0;

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
        return GetMessageByIndex(messageIndex).Text;
    }

    /// <summary>
    /// Gets the segments for the message at the specified index.
    /// </summary>
    public IReadOnlyList<ChatHistory.Message.MessageSegment> GetSegments(int messageIndex)
    {
        return GetMessageByIndex(messageIndex).Segments;
    }

    /// <summary>
    /// Gets the segments for the current message.
    /// </summary>
    public IReadOnlyList<ChatHistory.Message.MessageSegment> Segments => LMKitMessage?.Segments ?? Array.Empty<ChatHistory.Message.MessageSegment>();

    /// <summary>
    /// Checks if the message has any internal reasoning (thinking) content.
    /// </summary>
    public bool HasThinkingContent
    {
        get
        {
            if (LMKitMessage?.Segments == null)
            {
                return false;
            }

            return LMKitMessage.Segments.Any(s => s.SegmentType == TextSegmentType.InternalReasoning);
        }
    }

    /// <summary>
    /// Checks if the message is currently in thinking mode (last segment is InternalReasoning and message is in progress).
    /// </summary>
    public bool IsCurrentlyThinking
    {
        get
        {
            if (!MessageInProgress || LMKitMessage?.Segments == null || LMKitMessage.Segments.Count == 0)
            {
                return false;
            }

            return LMKitMessage.Segments.Last().SegmentType == TextSegmentType.InternalReasoning;
        }
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
        Content = LMKitMessage.Text;

        // Extract attachments from LMKit message
        if (message.Attachments != null && message.Attachments.Count > 0)
        {
            List<ChatAttachment> chatAttachments = new List<ChatAttachment>();
            foreach (var attachment in message.Attachments)
            {
                ChatAttachment chatAttachment = ChatAttachment.FromBytes(attachment.Target.Name, attachment.Target.Mime, attachment.Target.GetData());
            }

            Attachments = chatAttachments;
        }
    }

    public MessageViewModel(ConversationViewModel parentConversation, MessageSender sender, string content = "", IList<ChatAttachment>? attachments = null)
    {
        ParentConversation = parentConversation;
        Sender = sender;
        Content = content;
        LMKitMessage = new ChatHistory.Message(sender == MessageSender.User ? AuthorRole.User : AuthorRole.Assistant, content);

        // Store attachments for display
        if (attachments != null && attachments.Count > 0)
        {
            Attachments = attachments.ToList();
        }
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
            OnPropertyChanged(nameof(IsCurrentlyThinking));
        }
        else if (e.PropertyName == nameof(ChatHistory.Message.AuthorRole))
        {
            Sender = AuthorRoleToMessageSender(LMKitMessage!.AuthorRole);
        }
        else if (e.PropertyName == nameof(ChatHistory.Message.Text))
        {
            Content = LMKitMessage!.Text;
            OnPropertyChanged(nameof(Segments));
            OnPropertyChanged(nameof(HasThinkingContent));
            OnPropertyChanged(nameof(IsCurrentlyThinking));
        }
        else if (e.PropertyName == nameof(ChatHistory.Message.Segments))
        {
            OnPropertyChanged(nameof(Segments));
            OnPropertyChanged(nameof(HasThinkingContent));
            OnPropertyChanged(nameof(IsCurrentlyThinking));
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