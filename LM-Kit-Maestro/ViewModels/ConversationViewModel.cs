using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.TextGeneration;
using LMKit.TextGeneration.Chat;
using LMKitMaestro.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LMKitMaestro.Data;
using LMKitMaestro.Services;

namespace LMKitMaestro.ViewModels;

public partial class ConversationViewModel : AssistantSessionViewModelBase
{
    private readonly IMainThread _mainThread;
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILMKitMaestroDatabase _database;
    private readonly LMKitService.Conversation _lmKitConversation;

    private bool _isSynchedWithLog = true;
    private bool _pendingCancellation;

    private bool _awaitingLMKitUserMessage;
    private bool _awaitingLMKitAssistantMessage;
    private MessageViewModel? _pendingPrompt;
    private MessageViewModel? _pendingResponse;

    [ObservableProperty]
    bool _usedDifferentModel;

    [ObservableProperty]
    bool _logsLoadingInProgress;

    [ObservableProperty]
    bool _isInitialized;

    [ObservableProperty]
    public LmKitTextGenerationStatus _latestPromptStatus;

    [ObservableProperty]
    bool _isSelected;

    [ObservableProperty]
    bool _isHovered;

    [ObservableProperty]
    bool _isRenaming;

    [ObservableProperty]
    bool _isShowingActionPopup;

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
                ConversationLog.LastUsedModel = _lastUsedModel?.LocalPath;

                OnPropertyChanged();
            }
        }
    }

    public EventHandler? TextGenerationCompleted;
    public EventHandler? TextGenerationFailed;
    public EventHandler? DatabaseSaveOperationCompleted;
    public EventHandler? DatabaseSaveOperationFailed;

    public ConversationViewModel(IMainThread mainThread, IPopupService popupService, IAppSettingsService appSettingsService, LMKitService lmKitService, ILMKitMaestroDatabase database) : this(mainThread, popupService, appSettingsService, lmKitService, database, new ConversationLog("Untitled conversation"))
    {
    }

    public ConversationViewModel(IMainThread mainThread, IPopupService popupService, IAppSettingsService appSettingsService, LMKitService lmKitService, ILMKitMaestroDatabase database, ConversationLog conversationLog) : base(popupService, lmKitService)
    {
        _mainThread = mainThread;
        _appSettingsService = appSettingsService;
        _lmKitService = lmKitService;
        _lmKitService.ModelLoadingCompleted += OnModelLoadingCompleted;
        _lmKitService.ModelUnloaded += OnModelUnloaded;
        _database = database;
        _popupService = popupService;
        _title = conversationLog.Title!;
        _lmKitConversation = new LMKitService.Conversation(lmKitService, conversationLog.ChatHistoryData);
        _lmKitConversation.ChatHistoryChanged += OnLmKitChatHistoryChanged;
        _lmKitConversation.SummaryTitleGenerated += OnConversationSummaryTitleGenerated;
        _lmKitConversation.PropertyChanged += OnLmKitConversationPropertyChanged;
        Messages.CollectionChanged += OnMessagesCollectionChanged;
        ConversationLog = conversationLog;
        IsInitialized = conversationLog.ChatHistoryData == null;
    }

    public void LoadConversationLogs()
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
                        Messages.Add(new MessageViewModel(message) { MessageInProgress = false });
                    }
                }

                if (ConversationLog.LastUsedModel != null)
                {
                    LastUsedModel = new Uri(ConversationLog.LastUsedModel);
                }
            }
        }
        catch (Exception)
        {

        }

        IsInitialized = true;
    }

    public async Task Delete()
    {
        if (AwaitingResponse)
        {
            await CancelPendingPrompt(true);
        }
    }

    protected override void HandleSubmit()
    {
        string prompt = InputText;
        OnNewlySubmittedPrompt(prompt);

        LMKitService.PromptResult? promptResult = null;

        Task.Run(async () =>
        {
            try
            {
                promptResult = await _lmKitService.SubmitPrompt(_lmKitConversation, prompt);
                OnPromptResult(promptResult);
            }
            catch (Exception ex)
            {
                OnPromptResult(null, ex);
            }
        });
    }

    [RelayCommand]
    public async Task Cancel()
    {
        if (AwaitingResponse)
        {
            await CancelPendingPrompt(false);
        }
    }

    private void OnPromptResult(LMKitService.PromptResult? promptResult, Exception? submitPromptException = null)
    {
        AwaitingResponse = false;

        if (submitPromptException != null)
        {
            if (submitPromptException is OperationCanceledException operationCancelledException)
            {
                _pendingResponse!.Status = LmKitTextGenerationStatus.Cancelled;
                _pendingPrompt!.Status = LmKitTextGenerationStatus.Cancelled;
            }
            else
            {
                _pendingResponse!.Status = LmKitTextGenerationStatus.UnknownError;
                _pendingPrompt!.Status = LmKitTextGenerationStatus.UnknownError;
            }

            // todo: provide more error info with event args.
            OnTextGenerationFailure();
        }
        else if (promptResult != null)
        {
            LatestPromptStatus = promptResult.Status;
            _pendingResponse!.Status = LatestPromptStatus;
            _pendingPrompt!.Status = LatestPromptStatus;

            if (promptResult.Status == LmKitTextGenerationStatus.Undefined && promptResult.TextGenerationResult != null)
            {
                OnTextGenerationSuccess(promptResult.TextGenerationResult);
            }
            else
            {
                OnTextGenerationFailure();
            }
        }

        if (!_isSynchedWithLog)
        {
            SaveConversation();
            _isSynchedWithLog = true;
        }

        if (!_awaitingLMKitAssistantMessage)
        {
            _pendingResponse = null;
        }

        if (!_awaitingLMKitUserMessage)
        {
            _pendingPrompt = null;
        }

        _pendingCancellation &= false;
    }

    private async Task CancelPendingPrompt(bool shouldAwaitTermination)
    {
        _pendingCancellation = true;
        await _lmKitService.CancelPrompt(_lmKitConversation, shouldAwaitTermination);
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

    private void OnNewlySubmittedPrompt(string prompt)
    {
        InputText = string.Empty;
        UsedDifferentModel &= false;
        LatestPromptStatus = LmKitTextGenerationStatus.Undefined;
        AwaitingResponse = true;
        _awaitingLMKitUserMessage = true;
        _awaitingLMKitAssistantMessage = true;
        _pendingPrompt = new MessageViewModel(new Message() { Sender = MessageSender.User, Text = prompt });
        _pendingResponse = new MessageViewModel(new Message() { Sender = MessageSender.Assistant }) { MessageInProgress = true };

        Messages.Add(_pendingPrompt);
        Messages.Add(_pendingResponse);
    }

    private void OnTextGenerationSuccess(TextGenerationResult result)
    {
        TextGenerationCompleted?.Invoke(this, new TextGenerationCompletedEventArgs(result.TerminationReason));
    }

    private void OnTextGenerationFailure()
    {
        if (_pendingResponse != null)
        {
            _pendingResponse.MessageInProgress = false;
        }

        TextGenerationFailed?.Invoke(this, EventArgs.Empty);
    }

    private void OnLmKitChatHistoryChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _isSynchedWithLog &= false;

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems!)
            {
                ChatHistory.Message message = (ChatHistory.Message)item;

                if (message.AuthorRole == AuthorRole.User)
                {
                    if (_pendingPrompt != null && _awaitingLMKitUserMessage)
                    {
                        _pendingPrompt.LmKitMessage = message;
                        _awaitingLMKitUserMessage = false;

                        if (!AwaitingResponse)
                        {
                            _pendingPrompt = null;
                        }
                    }
                }
                else if (message.AuthorRole == AuthorRole.Assistant)
                {
                    if (_pendingResponse != null && _awaitingLMKitAssistantMessage)
                    {
                        _pendingResponse.LmKitMessage = message;
                        _awaitingLMKitUserMessage = false;

                        if (!AwaitingResponse)
                        {
                            _pendingResponse = null;
                        }
                    }
                }
                else
                {
                    MessageViewModel messageViewModel = new MessageViewModel(message);
                    _mainThread.BeginInvokeOnMainThread(() => Messages.Add(messageViewModel));
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (var item in e.OldItems!)
            {
                _mainThread.BeginInvokeOnMainThread(() => Messages.RemoveAt(e.OldStartingIndex));
            }
        }
    }

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IsEmpty = Messages.Count == 0;
    }

    private void OnConversationSummaryTitleGenerated(object? sender, EventArgs e)
    {
        if (Title == "Untitled conversation")
        {
            Title = _lmKitConversation.GeneratedTitleSummary!;
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
            await CancelPendingPrompt(false);
        }

        UsedDifferentModel = false;
    }

    private void OnLmKitConversationPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LMKitService.Conversation.LastUsedModelUri))
        {
            LastUsedModel = _lmKitConversation.LastUsedModelUri;
        }
        else if (e.PropertyName == nameof(LMKitService.Conversation.LatestChatHistoryData))
        {
            ConversationLog.ChatHistoryData = _lmKitConversation.LatestChatHistoryData;
        }
    }

    public sealed class TextGenerationCompletedEventArgs : EventArgs
    {
        public TextGenerationResult.StopReason StopReason { get; }

        public TextGenerationCompletedEventArgs(TextGenerationResult.StopReason stopReason)
        {
            StopReason = stopReason;
        }
    }
}
