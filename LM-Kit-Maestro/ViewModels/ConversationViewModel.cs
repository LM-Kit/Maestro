using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.TextGeneration;
using LMKit.TextGeneration.Chat;
using LMKit.Maestro.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LMKit.Maestro.Data;
using LMKit.Maestro.Services;

namespace LMKit.Maestro.ViewModels;

public partial class ConversationViewModel : AssistantSessionViewModelBase
{
    private readonly IMainThread _mainThread;
    private readonly IAppSettingsService _appSettingsService;
    private readonly IMaestroDatabase _database;
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
    public LMKitTextGenerationStatus _latestPromptStatus;

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

    public ConversationViewModel(IMainThread mainThread, IPopupService popupService, IAppSettingsService appSettingsService, LMKitService lmKitService, IMaestroDatabase database) : this(mainThread, popupService, appSettingsService, lmKitService, database, new ConversationLog("Untitled conversation"))
    {
    }

    public ConversationViewModel(IMainThread mainThread, IPopupService popupService, IAppSettingsService appSettingsService, LMKitService lmKitService, IMaestroDatabase database, ConversationLog conversationLog) : base(popupService, lmKitService)
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
        _lmKitConversation.ChatHistoryChanged += OnLMKitChatHistoryChanged;
        _lmKitConversation.SummaryTitleGenerated += OnConversationSummaryTitleGenerated;
        _lmKitConversation.PropertyChanged += OnLMKitConversationPropertyChanged;
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
                        Messages.Add(new MessageViewModel(this, message) { MessageInProgress = false });
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
            await HandleCancel(true);
        }
    }

    [RelayCommand]
    private async Task RegenerateResponse(MessageViewModel message)
    {
        var response = await _lmKitService.RegenerateResponse(_lmKitConversation, message.LMKitMessage!);

        OnResponseRegenerated();
    }

    protected override void HandleSubmit()
    {
        string prompt = InputText;
        OnNewlySubmittedPrompt(prompt);

        LMKitService.LMKitResult? promptResult = null;

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

    private void OnResponseRegenerating(MessageViewModel message)
    {
        message.Text = string.Empty;
        message.MessageInProgress = true;
        AwaitingResponse = true;
        _awaitingLMKitAssistantMessage = true;
    }

    private void OnNewlySubmittedPrompt(string prompt)
    {
        InputText = string.Empty;
        UsedDifferentModel &= false;
        LatestPromptStatus = LMKitTextGenerationStatus.Undefined;
        AwaitingResponse = true;
        _awaitingLMKitUserMessage = true;
        _awaitingLMKitAssistantMessage = true;
        _pendingPrompt = new MessageViewModel(this, new Message() { Sender = MessageSender.User, Text = prompt });
        _pendingResponse = new MessageViewModel(this, new Message() { Sender = MessageSender.Assistant }) { MessageInProgress = true };

        Messages.Add(_pendingPrompt);
        Messages.Add(_pendingResponse);
    }

    private void OnResponseRegenerated()
    {

    }
    private void OnPromptResult(LMKitService.LMKitResult? promptResult, Exception? submitPromptException = null)
    {
        AwaitingResponse = false;

        if (submitPromptException != null)
        {
            if (submitPromptException is OperationCanceledException operationCancelledException)
            {
                _pendingResponse!.Status = LMKitTextGenerationStatus.Cancelled;
                _pendingPrompt!.Status = LMKitTextGenerationStatus.Cancelled;
            }
            else
            {
                _pendingResponse!.Status = LMKitTextGenerationStatus.UnknownError;
                _pendingPrompt!.Status = LMKitTextGenerationStatus.UnknownError;
            }

            // todo: provide more error info with event args.
            OnTextGenerationFailure();
        }
        else if (promptResult != null)
        {
            LatestPromptStatus = promptResult.Status;
            _pendingResponse!.Status = LatestPromptStatus;
            _pendingPrompt!.Status = LatestPromptStatus;

            if (promptResult.Status == LMKitTextGenerationStatus.Undefined && promptResult.Result is TextGenerationResult textGenerationResult)
            {
                OnTextGenerationSuccess(textGenerationResult);
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

    protected override async Task HandleCancel(bool shouldAwaitTermination)
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

    private void OnLMKitChatHistoryChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
                        _pendingPrompt.LMKitMessage = message;
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
                        _pendingResponse.LMKitMessage = message;
                        _awaitingLMKitUserMessage = false;

                        if (!AwaitingResponse)
                        {
                            _pendingResponse = null;
                        }
                    }
                }
                else
                {
                    MessageViewModel messageViewModel = new MessageViewModel(this, message);
                    _mainThread.BeginInvokeOnMainThread(() => Messages.Add(messageViewModel));
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            int count = 0;

            foreach (var item in e.OldItems!)
            {
                _mainThread.BeginInvokeOnMainThread(() => Messages.RemoveAt(e.OldStartingIndex - e.OldItems.Count + count));
                count++;
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Replace)
        {

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
            await HandleCancel(false);
        }

        UsedDifferentModel = false;
    }

    private void OnLMKitConversationPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
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
