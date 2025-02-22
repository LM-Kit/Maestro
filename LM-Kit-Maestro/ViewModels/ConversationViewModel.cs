using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.TextGeneration;
using LMKit.TextGeneration.Chat;
using LMKit.Maestro.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LMKit.Maestro.Data;
using LMKit.Maestro.Services;
using LMKit.Maestro.UI;

namespace LMKit.Maestro.ViewModels;

public partial class ConversationViewModel : AssistantViewModelBase
{
    private readonly IMaestroDatabase _database;

    public LMKitService.Conversation LMKitConversation { get; private set; }

    private MessageViewModel? _pendingPrompt;
    private MessageViewModel? _pendingResponse;
    private bool _isSynchedWithLog = true;

    [ObservableProperty] bool _isEmpty = true;

    [ObservableProperty] bool _usedDifferentModel;

    [ObservableProperty] bool _logsLoadingInProgress;

    [ObservableProperty] bool _isInitialized;

    [ObservableProperty] public LMKitRequestStatus _latestPromptStatus;

    [ObservableProperty] bool _isSelected;

    [ObservableProperty] bool _isHovered;

    [ObservableProperty] bool _isRenaming;

    [ObservableProperty] bool _isShowingActionPopup;

    public ObservableCollection<MessageViewModel> Messages { get; } = new ObservableCollection<MessageViewModel>();
    public ConversationLog ConversationLog { get; }

    private string _title;

    public string Title
    {
        get => _title;
        set
        {
            if (value != _title)
            {
                ConversationLog.Title = value;
                _title = value;
                OnPropertyChanged();

                SaveConversation();
            }
        }
    }

    private Uri? _lastUsedModel;

    public Uri? LastUsedModel
    {
        get => _lastUsedModel;
        set
        {
            if (_lastUsedModel != value)
            {
                _lastUsedModel = value;
                UsedDifferentModel = LastUsedModel != _lmKitService.LMKitConfig.LoadedModelUri;
                ConversationLog.LastUsedModel = _lastUsedModel;

                OnPropertyChanged();
            }
        }
    }

    public delegate void TextGenerationCompletedEventHandler(object sender, TextGenerationCompletedEventArgs e);

    public TextGenerationCompletedEventHandler? TextGenerationCompleted;
    public EventHandler? DatabaseSaveOperationCompleted;
    public EventHandler? DatabaseSaveOperationFailed;

    public ConversationViewModel(IPopupService popupService, LMKitService lmKitService, IMaestroDatabase database) :
        this(popupService, lmKitService, database, new ConversationLog(Locales.UntitledChat))
    {
    }

    public ConversationViewModel(IPopupService popupService, LMKitService lmKitService, IMaestroDatabase database,
        ConversationLog conversationLog) : base(popupService, lmKitService)
    {
        _lmKitService = lmKitService;
        _lmKitService.ModelLoaded += OnModelLoadingCompleted;
        _lmKitService.ModelUnloaded += OnModelUnloaded;
        _database = database;
        _popupService = popupService;
        _title = conversationLog.Title!;
        LMKitConversation = new LMKitService.Conversation(lmKitService, conversationLog.ChatHistoryData);
        LMKitConversation.ChatHistoryChanged += OnLMKitChatHistoryChanged;
        LMKitConversation.SummaryTitleGenerated += OnConversationSummaryTitleGenerated;
        LMKitConversation.PropertyChanged += OnLMKitConversationPropertyChanged;
        Messages.CollectionChanged += OnMessagesCollectionChanged;
        ConversationLog = conversationLog;
        IsInitialized = conversationLog.ChatHistoryData == null;
    }

    public async Task Delete()
    {
        if (AwaitingResponse)
        {
            await HandleCancel(true);
        }
    }

    public bool LoadConversationLog()
    {
        try
        {
            if (ConversationLog.ChatHistoryData != null)
            {
                var chatHistory = ChatHistory.Deserialize(ConversationLog.ChatHistoryData);

                foreach (var message in chatHistory.Messages)
                {
                    if (message.AuthorRole == AuthorRole.Assistant || message.AuthorRole == AuthorRole.User)
                    {
                        Messages.Add(new MessageViewModel(this, message) { MessageInProgress = false });
                    }
                }

                SetLastAssistantMessage();

                if (ConversationLog.LastUsedModel != null)
                {
                    LastUsedModel = ConversationLog.LastUsedModel;
                }
            }

            IsInitialized = true;

            return true;
        }
        catch (Exception? ex)
        {
            return false;
        }
    }

    [RelayCommand]
    private void RegenerateResponse(MessageViewModel message)
    {
        if (_lmKitService.ModelLoadingState != LMKitModelLoadingState.Loaded)
        {
            _popupService.DisplayAlert("No model is loaded",
                "You need to load a model in order to regenerate a response", "OK");
        }
        else
        {
            OnResponseRegenerationRequested(message);

            Task.Run(async () =>
            {
                LMKitService.LMKitResult? result = null;

                try
                {
                    result = await _lmKitService.Chat.RegenerateResponse(LMKitConversation, message.LMKitMessage!);
                    OnTextGenerationResult(result);
                }
                catch (Exception exception)
                {
                    OnTextGenerationResult(null, exception);
                }
            });
        }
    }

    protected override void HandleSubmit()
    {
        string prompt = InputText;

        OnNewlySubmittedPrompt();

        LMKitService.LMKitResult? promptResult = null;

        Task.Run(async () =>
        {
            try
            {
                promptResult = await _lmKitService.Chat.SubmitPrompt(LMKitConversation, prompt);
                OnTextGenerationResult(promptResult);
            }
            catch (Exception ex)
            {
                OnTextGenerationResult(null, ex);
            }
        });
    }

    private void SetLastAssistantMessage()
    {
        var lastAssistantMessages = Messages.Where(message => message.Sender == MessageSender.Assistant).ToList();

        lastAssistantMessages = lastAssistantMessages.Skip(Math.Max(lastAssistantMessages.Count - 2, 0)).ToList();

        if (lastAssistantMessages.Count > 0)
        {
            lastAssistantMessages[lastAssistantMessages.Count - 1].IsLastAssistantMessage = true;

            if (lastAssistantMessages.Count == 2)
            {
                lastAssistantMessages[0].IsLastAssistantMessage = false;
            }
        }
    }

    private void OnResponseRegenerationRequested(MessageViewModel message)
    {
        AwaitingResponse = true;
    }

    private void OnNewlySubmittedPrompt()
    {
        _pendingPrompt = new MessageViewModel(this, MessageSender.User, InputText);
        _pendingResponse = new MessageViewModel(this, MessageSender.Assistant) { MessageInProgress = true };
        Messages.Add(_pendingPrompt);
        Messages.Add(_pendingResponse);

        InputText = string.Empty;
        UsedDifferentModel &= false;
        LatestPromptStatus = LMKitRequestStatus.OK;
        AwaitingResponse = true;
    }

    private void OnTextGenerationResult(LMKitService.LMKitResult? result, Exception? exception = null)
    {
        AwaitingResponse = false;

        if (Messages.Count >= 2)
        {
            // Setting error status for the last assistant message if the response generation failed.
            Messages.Last().Status = result != null ? result.Status :
                exception is OperationCanceledException ? LMKitRequestStatus.Cancelled :
                LMKitRequestStatus.GenericError;
        }

        var textGenerationResult = result?.Result is TextGenerationResult ? (TextGenerationResult)result.Result : null;

        TextGenerationCompleted?.Invoke(this,
            new TextGenerationCompletedEventArgs(
                result?.Result is TextGenerationResult ? (TextGenerationResult)result.Result : null,
                exception ?? (result?.Exception), result?.Status));

        if (!_isSynchedWithLog)
        {
            SaveConversation();
            _isSynchedWithLog = true;
        }
    }

    protected override async Task HandleCancel(bool shouldAwaitTermination)
    {
        if (_pendingResponse != null)
        {
            _pendingResponse.MessageInProgress = false;
            _pendingResponse.Status = LMKitRequestStatus.Cancelled;
            _pendingPrompt = null;
            _pendingPrompt = null;
        }

        await _lmKitService.Chat.CancelPrompt(LMKitConversation, shouldAwaitTermination);
    }

    private void SaveConversation()
    {
        Task.Run(async () =>
        {
            try
            {
                await _database.SaveConversation(ConversationLog);

                DatabaseSaveOperationCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
                DatabaseSaveOperationFailed?.Invoke(this, EventArgs.Empty);
            }
        });
    }

    private void OnLMKitChatHistoryChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _isSynchedWithLog &= false;

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems!)
            {
                var message = (ChatHistory.Message)item;

                if (message.AuthorRole == AuthorRole.System)
                {
                    continue;
                }

                if (message.AuthorRole == AuthorRole.User && _pendingPrompt != null)
                {
                    _pendingPrompt.LMKitMessage = message;
                    _pendingPrompt = null;
                }
                else if (message.AuthorRole == AuthorRole.Assistant && _pendingResponse != null)
                {
                    _pendingResponse.LMKitMessage = message;
                    _pendingResponse = null;
                }
                else
                {
                    Messages.Add(new MessageViewModel(this, message));
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            int count = 0;

            foreach (var item in e.OldItems!)
            {
                Messages.RemoveAt(e.OldStartingIndex - e.OldItems.Count + count);
                count++;
            }
        }

        SetLastAssistantMessage();
    }

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IsEmpty = Messages.Count == 0;
    }

    private void OnConversationSummaryTitleGenerated(object? sender, EventArgs e)
    {
        if (Title == Locales.UntitledChat)
        {
            Title = LMKitConversation.GeneratedTitleSummary!;
        }
    }

    private void OnModelLoadingCompleted(object? sender, EventArgs e)
    {
        if (LastUsedModel != null)
        {
            UsedDifferentModel = LastUsedModel != _lmKitService.LMKitConfig.LoadedModelUri;
        }
    }

    private async void OnModelUnloaded(object? sender, EventArgs e)
    {
        if (AwaitingResponse)
        {
            await HandleCancel(false);
        }

        UsedDifferentModel = false;
    }

    private void OnLMKitConversationPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LMKitService.Conversation.LastUsedModelUri))
        {
            LastUsedModel = LMKitConversation.LastUsedModelUri;
        }
        else if (e.PropertyName == nameof(LMKitService.Conversation.LatestChatHistoryData))
        {
            ConversationLog.ChatHistoryData = LMKitConversation.LatestChatHistoryData;
        }
    }

    public sealed class TextGenerationCompletedEventArgs : EventArgs
    {
        public Exception? Exception { get; }

        public LMKitRequestStatus? Status { get; }

        public TextGenerationResult? Result { get; }

        public TextGenerationCompletedEventArgs(TextGenerationResult? result = null, Exception? exception = null,
            LMKitRequestStatus? status = null)
        {
            Result = result;
            Exception = exception;
            Status = status;
        }
    }
}