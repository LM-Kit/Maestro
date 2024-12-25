﻿using LMKit.TextGeneration;
using LMKit.TextGeneration.Chat;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    public sealed class Conversation : INotifyPropertyChanged
    {
        private ChatHistory? _chatHistory;
        public ChatHistory? ChatHistory
        {
            get => _chatHistory;
            set
            {
                if (_chatHistory != value)
                {
                    if (_chatHistory != null)
                    {
                        ((INotifyCollectionChanged)_chatHistory.Messages).CollectionChanged -= OnChatHistoryMessageCollectionChanged;
                    }

                    _chatHistory = value;

                    if (_chatHistory != null)
                    {
                        ((INotifyCollectionChanged)_chatHistory.Messages).CollectionChanged += OnChatHistoryMessageCollectionChanged;
                    }

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChatHistory)));
                }
            }
        }

        private string? _generatedTitle;
        public string? GeneratedTitleSummary
        {
            get => _generatedTitle;
            set
            {
                if (_generatedTitle != value)
                {
                    _generatedTitle = value;

                    if (_generatedTitle != null)
                    {
                        SummaryTitleGenerated?.Invoke(this, EventArgs.Empty);
                    }

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GeneratedTitleSummary)));
                }
            }
        }

        private Uri? _lastUsedModelUri;
        public Uri? LastUsedModelUri
        {
            get => _lastUsedModelUri;
            set
            {
                if (_lastUsedModelUri != value)
                {
                    _lastUsedModelUri = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastUsedModelUri)));
                }
            }
        }

        private byte[]? _latestChatHistoryData;
        public byte[]? LatestChatHistoryData
        {
            get => _latestChatHistoryData;
            set
            {
                if (_latestChatHistoryData != value)
                {
                    _latestChatHistoryData = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LatestChatHistoryData)));
                }
            }
        }

        private int _contextSize;

        public int ContextSize
        {
            get => _contextSize;
            private set
            {
                if (_contextSize != value)
                {
                    _contextSize = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContextSize)));
                }
            }
        }

        private int _contextRemainingSpace;

        public int ContextRemainingSpace
        {
            get => _contextRemainingSpace;
            private set
            {
                if (_contextRemainingSpace != value)
                {
                    _contextRemainingSpace = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContextRemainingSpace)));
                }
            }
        }

        private bool _inTextCompletion;

        public bool InTextCompletion
        {
            get => _inTextCompletion;
            set
            {
                if (_inTextCompletion != value)
                {
                    _inTextCompletion = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InTextCompletion)));
                }
            }
        }

        public int ContextUsedSpace
        {
            get
            {
                return _contextSize - _contextRemainingSpace;
            }
        }

        public event EventHandler? SummaryTitleGenerated;
        public event NotifyCollectionChangedEventHandler? ChatHistoryChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public Conversation(LMKitService lmKitService, byte[]? latestChatHistoryData = null)
        {
            lmKitService.ModelUnloaded += OnModelUnloaded;
            LatestChatHistoryData = latestChatHistoryData;
        }

        private void OnModelUnloaded(object? sender, EventArgs e)
        {
            if (ChatHistory != null)
            {
                // Making sure that the chat history is re-built with the next loaded model information.
                ChatHistory = null;
            }

            ContextSize = 0;
            ContextRemainingSpace = 0;
        }

        private void OnChatHistoryMessageCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ChatHistoryChanged?.Invoke(this, e);
        }

        public void SetGeneratedTitle(LMKitResult result)
        {
            string? conversationTopic = null;

            if (result.Result != null && result.Result is Summarizer.SummarizerResult textGenerationResult &&
                !string.IsNullOrEmpty(textGenerationResult.Title))
            {
                conversationTopic = textGenerationResult.Title;
            }

            var firstUserMessage = ChatHistory!.Messages.FirstOrDefault(message => message.AuthorRole == AuthorRole.User);

            GeneratedTitleSummary = !string.IsNullOrWhiteSpace(conversationTopic) ? conversationTopic : firstUserMessage?.Content ?? "Untitled conversation";
        }

        internal void AfterTokenSampling(object? sender, TextGeneration.Events.AfterTokenSamplingEventArgs e)
        {
            ContextSize = e.ContextSize;
            ContextRemainingSpace = e.ContextRemainingSpace;
            InTextCompletion = true;
        }
    }
}
