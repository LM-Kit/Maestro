using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.TextGeneration;
using LMKit.TextGeneration.Chat;
using LMKit.Maestro.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LMKit.Maestro.Data;
using LMKit.Maestro.Services;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LMKit.Maestro.ViewModels;

public partial class ConversationViewModel : AssistantViewModelBase
{
    private readonly IMainThread _mainThread;
    private readonly IAppSettingsService _appSettingsService;
    private readonly IMaestroDatabase _database;

    public LMKitService.Conversation LMKitConversation { get; private set; }


    private bool _isSynchedWithLog = true;

    [ObservableProperty]
    bool _isEmpty = true;

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
        LMKitConversation = new LMKitService.Conversation(lmKitService, conversationLog.ChatHistoryData);
        LMKitConversation.ChatHistoryChanged += OnLMKitChatHistoryChanged;
        LMKitConversation.SummaryTitleGenerated += OnConversationSummaryTitleGenerated;
        LMKitConversation.PropertyChanged += OnLMKitConversationPropertyChanged;
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

                SetLastAssistantMessage();
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
    private void RegenerateResponse(MessageViewModel message)
    {
        if (_lmKitService.ModelLoadingState != LMKitModelLoadingState.Loaded)
        {
            _popupService.DisplayAlert("No model is loaded", "You need to load a model in order to regenerate a response", "OK");
        }
        else
        {
            OnResponseRegenerationRequested(message);

            Task.Run(async () =>
            {
                LMKitService.LMKitResult? result = null;

                try
                {
                    result = await _lmKitService.RegenerateResponse(LMKitConversation, message.LMKitMessage!);
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
                promptResult = await _lmKitService.SubmitPrompt(LMKitConversation, prompt);
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
        try
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
        catch (Exception)
        {

        }
    }

    private void OnResponseRegenerationRequested(MessageViewModel message)
    {
        AwaitingResponse = true;
    }

    private void OnNewlySubmittedPrompt()
    {
        InputText = string.Empty;
        UsedDifferentModel &= false;
        LatestPromptStatus = LMKitTextGenerationStatus.Undefined;
        AwaitingResponse = true;
    }

    private void OnTextGenerationResult(LMKitService.LMKitResult? result, Exception? exception = null)
    {
        AwaitingResponse = false;

        if (Messages.Count >= 2)
        {
            // An error may occur before messages consecutive from a prompt have been added to the list, add count check.  
            Messages.Last().Status = result != null ? result.Status : exception is OperationCanceledException ? LMKitTextGenerationStatus.Cancelled : LMKitTextGenerationStatus.GenericError;
        }

        if (exception != null || result?.Exception != null)
        {
            // todo: provide more error info with event args.
            OnTextGenerationFailure();
        }
        else if (result != null)
        {
            if (result.Status == LMKitTextGenerationStatus.Undefined && result.Result is TextGenerationResult textGenerationResult)
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
    }

    protected override async Task HandleCancel(bool shouldAwaitTermination)
    {
        await _lmKitService.CancelPrompt(LMKitConversation, shouldAwaitTermination);
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
        TextGenerationFailed?.Invoke(this, EventArgs.Empty);
    }

    private void OnLMKitChatHistoryChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _isSynchedWithLog &= false;

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems!)
            {
                Messages.Add(new MessageViewModel(this, (ChatHistory.Message)item));
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
        if (Title == "Untitled conversation")
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
        public TextGenerationResult.StopReason StopReason { get; }

        public TextGenerationCompletedEventArgs(TextGenerationResult.StopReason stopReason)
        {
            StopReason = stopReason;
        }
    }
}
