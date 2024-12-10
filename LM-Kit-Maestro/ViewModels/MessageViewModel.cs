﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Models;
using LMKit.Maestro.Services;
using LMKit.TextGeneration.Chat;
using static LMKit.TextGeneration.TextGenerationResult;

namespace LMKit.Maestro.ViewModels;

public partial class MessageViewModel : ViewModelBase
{
    public ConversationViewModel ParentConversation { get; }

    private ChatHistory.Message? _lmKitMessage;

    public ChatHistory.Message? LMKitMessage
    {
        get => _lmKitMessage;
    }

    [ObservableProperty]
    private MessageSender _sender;

    [ObservableProperty]
    private string _content;

    [ObservableProperty]
    private bool _messageInProgress;

    [ObservableProperty]
    private LMKitTextGenerationStatus _status;

    [ObservableProperty]
    private bool _isHovered;

    [ObservableProperty]
    private bool _isLastAssistantMessage;

    // Loïc: We should not require introducing the MessageContentUpdated event as Content is observable
    public event EventHandler? MessageContentUpdated;

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
        if (_lmKitMessage != null)
        {
            if (_lmKitMessage.PreviousContent != null && _lmKitMessage.PreviousContent.Count > 0)
            {
                return _lmKitMessage.PreviousContent.Count + 1;
            }

            return 1;
        }

        throw new InvalidOperationException();
    }

    private ChatHistory.Message GetMessageByIndex(int index)
    {
        if (_lmKitMessage != null)
        {
            if (_lmKitMessage.PreviousContent != null && _lmKitMessage.PreviousContent.Count > 0)
            {
                if (index == _lmKitMessage.PreviousContent.Count)
                {
                    return _lmKitMessage;
                }
                else
                {
                    return _lmKitMessage.PreviousContent[index];
                }
            }
            else
            {
                return _lmKitMessage;
            }
        }

        throw new InvalidOperationException();
    }

    public MessageViewModel(ConversationViewModel parentConversation, ChatHistory.Message message)
    {
        ParentConversation = parentConversation;
        _lmKitMessage = message;
        MessageInProgress = !_lmKitMessage.IsProcessed;
        Sender = AuthorRoleToMessageSender(_lmKitMessage.AuthorRole);
        Content = _lmKitMessage.Content;
        _lmKitMessage.PropertyChanged += OnMessagePropertyChanged;
        Content = "";
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
            MessageInProgress = !_lmKitMessage!.IsProcessed;
        }
        else if (e.PropertyName == nameof(ChatHistory.Message.AuthorRole))
        {
            Sender = AuthorRoleToMessageSender(_lmKitMessage!.AuthorRole);
        }
        else if (e.PropertyName == nameof(ChatHistory.Message.Content))
        {
            Content = _lmKitMessage!.Content;
            // Loïc: We should not require introducing the MessageContentUpdated event as Content is observable
            MessageContentUpdated?.Invoke(this, EventArgs.Empty);
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