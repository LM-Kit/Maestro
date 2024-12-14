using LMKit.Model;
using LMKit.TextGeneration;
using LMKit.TextGeneration.Chat;
using LMKit.TextGeneration.Sampling;
using LMKit.Translation;
using System.ComponentModel;

namespace LMKit.Maestro.Services;

public partial class LMKitService : INotifyPropertyChanged
{
    private readonly SemaphoreSlim _lmKitServiceSemaphore = new SemaphoreSlim(1);

    private readonly RequestSchedule _titleGenerationSchedule = new RequestSchedule();
    private readonly RequestSchedule _requestSchedule = new RequestSchedule();

    private static Uri? _currentlyLoadingModelUri;
    private static Conversation? _lastConversationUsed = null;
    private static LLM? _model;
    private static MultiTurnConversation? _multiTurnConversation;


    public LMKitConfig LMKitConfig { get; } = new LMKitConfig();

    public event NotifyModelStateChangedEventHandler? ModelLoadingProgressed;
    public event NotifyModelStateChangedEventHandler? ModelLoadingCompleted;
    public event NotifyModelStateChangedEventHandler? ModelLoadingFailed;
    public event NotifyModelStateChangedEventHandler? ModelUnloaded;
    public event PropertyChangedEventHandler? PropertyChanged;

    public delegate void NotifyModelStateChangedEventHandler(object? sender, NotifyModelStateChangedEventArgs notifyModelStateChangedEventArgs);

    private LMKitModelLoadingState _modelLoadingState;
    public LMKitModelLoadingState ModelLoadingState
    {
        get => _modelLoadingState;
        set
        {
            _modelLoadingState = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModelLoadingState)));
        }
    }

    public void LoadModel(Uri fileUri, string? localFilePath = null)
    {
        if (_model != null)
        {
            UnloadModel();
        }

        _lmKitServiceSemaphore.Wait();
        _currentlyLoadingModelUri = fileUri;
        ModelLoadingState = LMKitModelLoadingState.Loading;

        var modelLoadingTask = new Task(() =>
        {
            bool modelLoadingSuccess;

            try
            {
                _model = new LLM(fileUri, fileUri.IsFile ? fileUri.LocalPath : localFilePath, loadingProgress: OnModelLoadingProgressed);

                modelLoadingSuccess = true;
            }
            catch (Exception exception)
            {
                modelLoadingSuccess = false;
                ModelLoadingFailed?.Invoke(this, new ModelLoadingFailedEventArgs(fileUri, exception));
            }
            finally
            {
                _currentlyLoadingModelUri = null;
                _lmKitServiceSemaphore.Release();
            }

            if (modelLoadingSuccess)
            {
                LMKitConfig.LoadedModelUri = fileUri!;

                ModelLoadingCompleted?.Invoke(this, new NotifyModelStateChangedEventArgs(LMKitConfig.LoadedModelUri));
                ModelLoadingState = LMKitModelLoadingState.Loaded;
            }
            else
            {
                ModelLoadingState = LMKitModelLoadingState.Unloaded;
            }

        });

        modelLoadingTask.Start();
    }

    public void UnloadModel()
    {
        // Ensuring we don't clean things up while a model is already being loaded,
        // or while the currently loaded model instance should not be touched
        // (while we are getting Lm-Kit objects ready to process a newly submitted prompt for instance).
        _lmKitServiceSemaphore.Wait();

        var unloadedModelUri = LMKitConfig.LoadedModelUri!;

        if (_requestSchedule.RunningPromptRequest != null && !_requestSchedule.RunningPromptRequest.CancellationTokenSource.IsCancellationRequested)
        {
            _requestSchedule.RunningPromptRequest.CancelAndAwaitTermination();
        }
        else if (_requestSchedule.Count > 1)
        {
            // A prompt is scheduled, but it is not running.
            _requestSchedule.Next!.CancelAndAwaitTermination();
        }

        if (_titleGenerationSchedule.RunningPromptRequest != null && !_titleGenerationSchedule.RunningPromptRequest.CancellationTokenSource.IsCancellationRequested)
        {
            _titleGenerationSchedule.RunningPromptRequest.CancelAndAwaitTermination();
        }
        else if (_requestSchedule.Count > 1)
        {
            _titleGenerationSchedule.Next!.CancelAndAwaitTermination();
        }

        if (_multiTurnConversation != null)
        {
            _multiTurnConversation.Dispose();
            _multiTurnConversation = null;
        }

        if (_model != null)
        {
            _model.Dispose();
            _model = null;
        }

        _lmKitServiceSemaphore.Release();

        _lastConversationUsed = null;
        ModelLoadingState = LMKitModelLoadingState.Unloaded;
        LMKitConfig.LoadedModelUri = null;

        ModelUnloaded?.Invoke(this, new NotifyModelStateChangedEventArgs(unloadedModelUri));
    }

    public async Task<string?> SubmitTranslation(string input, Language language)
    {
        var translationRequest = new LMKitRequest(LMKitRequest.LMKitRequestType.Translate,
            new LMKitRequest.TranslationRequestParameters(input, language), LMKitConfig.RequestTimeout);

        ScheduleRequest(translationRequest);

        var response = await HandleLMKitRequest(translationRequest);

        return (string?)response.Result;
    }

    public async Task<LMKitResult> SubmitPrompt(Conversation conversation, string prompt)
    {
        var promptRequest = new LMKitRequest(LMKitRequest.LMKitRequestType.Prompt,
            new LMKitRequest.PromptRequestParameters(conversation, prompt),
            LMKitConfig.RequestTimeout);

        ScheduleRequest(promptRequest);

        return await HandleLMKitRequest(promptRequest);
    }

    public async Task<LMKitResult> RegenerateResponse(Conversation conversation, ChatHistory.Message message)
    {
        var regenerateResponseRequest = new LMKitRequest(LMKitRequest.LMKitRequestType.RegenerateResponse,
            new LMKitRequest.RegenerateResponseParameters(conversation, message), LMKitConfig.RequestTimeout);

        ScheduleRequest(regenerateResponseRequest);

        return await HandleLMKitRequest(regenerateResponseRequest);
    }

    public async Task CancelPrompt(Conversation conversation, bool shouldAwaitTermination = false)
    {
        var conversationPrompt = _requestSchedule.Unschedule(conversation);

        if (conversationPrompt != null)
        {
            _lmKitServiceSemaphore.Wait();
            conversationPrompt.CancellationTokenSource.Cancel();
            conversationPrompt.ResponseTask.TrySetCanceled();
            _lmKitServiceSemaphore.Release();

            if (shouldAwaitTermination)
            {
                await conversationPrompt.ResponseTask.Task.WaitAsync(TimeSpan.FromSeconds(10));
            }

        }
    }

    private void ScheduleRequest(LMKitRequest request)
    {
        _requestSchedule.Schedule(request);

        if (_requestSchedule.Count > 1)
        {
            request.CanBeExecutedSignal.WaitOne();
        }
    }

    private async Task<LMKitResult> HandleLMKitRequest(LMKitRequest request)
    {
        // Ensuring we don't touch anything until Lm-Kit objects' state has been set to handle this request.
        _lmKitServiceSemaphore.Wait();

        LMKitResult result;

        if (request.CancellationTokenSource.IsCancellationRequested || ModelLoadingState == LMKitModelLoadingState.Unloaded)
        {
            result = new LMKitResult()
            {
                Status = LMKitTextGenerationStatus.Cancelled
            };

            _lmKitServiceSemaphore.Release();
        }
        else
        {
            if (request.RequestType == LMKitRequest.LMKitRequestType.Prompt || request.RequestType == LMKitRequest.LMKitRequestType.RegenerateResponse)
            {
                var conversation = request.RequestType == LMKitRequest.LMKitRequestType.Prompt ?
                    ((LMKitRequest.PromptRequestParameters)request.Parameters!).Conversation :
                    ((LMKitRequest.RegenerateResponseParameters)request.Parameters!).Conversation;

                BeforeSubmittingPrompt(conversation);
            }

            _lmKitServiceSemaphore.Release();


            result = await SubmitRequest(request);
        }

        if (_requestSchedule.Contains(request))
        {
            _requestSchedule.Remove(request);
        }

        request.ResponseTask.TrySetResult(result);

        return result;

    }

    private async Task<LMKitResult> SubmitRequest(LMKitRequest request)
    {
        try
        {
            _requestSchedule.RunningPromptRequest = request;

            var result = new LMKitResult();

            try
            {
                if (request.RequestType == LMKitRequest.LMKitRequestType.Prompt)
                {
                    result.Result = await _multiTurnConversation!.SubmitAsync(((LMKitRequest.PromptRequestParameters)request.Parameters!).Prompt,
                        request.CancellationTokenSource.Token);
                }
                else if (request.RequestType == LMKitRequest.LMKitRequestType.RegenerateResponse)
                {
                    result.Result = await _multiTurnConversation!.RegenerateResponseAsync(request.CancellationTokenSource.Token);
                }
                else if (request.RequestType == LMKitRequest.LMKitRequestType.Translate)
                {
                    TextTranslation textTranslation = new TextTranslation(_model);
                    var translationRequestParameters = (LMKitRequest.TranslationRequestParameters)request.Parameters!;
                    result.Result = await textTranslation!.TranslateAsync(translationRequestParameters.InputText,
                        translationRequestParameters.Language, request.CancellationTokenSource.Token);
                }
            }
            catch (Exception exception)
            {
                result.Exception = exception;

                if (result.Exception is OperationCanceledException)
                {
                    result.Status = LMKitTextGenerationStatus.Cancelled;
                }
                else
                {
                    result.Status = LMKitTextGenerationStatus.GenericError;
                }
            }

            if ((request.RequestType == LMKitRequest.LMKitRequestType.Prompt || request.RequestType == LMKitRequest.LMKitRequestType.RegenerateResponse) && _multiTurnConversation != null)
            {
                var promptParameters = request.Parameters as LMKitRequest.PromptRequestParameters;
                var regenerateParameters = request.Parameters as LMKitRequest.RegenerateResponseParameters;
                var conversation = promptParameters != null ? promptParameters.Conversation : regenerateParameters!.Conversation;

                conversation.ChatHistory = _multiTurnConversation.ChatHistory;

                conversation.LatestChatHistoryData = _multiTurnConversation.ChatHistory.Serialize();

                if (request.RequestType == LMKitRequest.LMKitRequestType.Prompt &&
                    conversation.GeneratedTitleSummary == null &&
                    result.Status == LMKitTextGenerationStatus.Undefined &&
                    !string.IsNullOrEmpty(((TextGenerationResult)result.Result!).Completion))
                {
                    GenerateConversationSummaryTitle(conversation, promptParameters!.Prompt);
                }
            }

            if (result.Exception != null && request.CancellationTokenSource.IsCancellationRequested)
            {
                result.Status = LMKitTextGenerationStatus.Cancelled;
            }

            return result;
        }
        catch (Exception exception)
        {
            return new LMKitResult()
            {
                Exception = exception,
                Status = LMKitTextGenerationStatus.GenericError
            };
        }
        finally
        {
            _requestSchedule.RunningPromptRequest = null;
        }
    }

    private void GenerateConversationSummaryTitle(Conversation conversation, string prompt)
    {
        LMKitRequest titleGenerationRequest = new LMKitRequest(LMKitRequest.LMKitRequestType.GenerateTitle,
            new LMKitRequest.PromptRequestParameters(conversation, prompt), 60);

        _titleGenerationSchedule.Schedule(titleGenerationRequest);

        if (_titleGenerationSchedule.Count > 1)
        {
            titleGenerationRequest.CanBeExecutedSignal.WaitOne();
        }

        _titleGenerationSchedule.RunningPromptRequest = titleGenerationRequest;

        Task.Run(async () =>
        {
            Summarizer summarizer = new Summarizer(_model)
            {
                MaximumContextLength = 512,
                GenerateContent = false,
                GenerateTitle = true,
                MaxTitleWords = 10,
                Guidance = "This content corresponds to the initial user message in a multi-turn conversation"
            };

            LMKitResult promptResult = new LMKitResult();

            try
            {
                promptResult.Result = await summarizer.SummarizeAsync(prompt, titleGenerationRequest.CancellationTokenSource.Token);
            }
            catch (Exception exception)
            {
                promptResult.Exception = exception;
            }
            finally
            {
                _titleGenerationSchedule.RunningPromptRequest = null;
                _titleGenerationSchedule.Remove(titleGenerationRequest);
                conversation.SetGeneratedTitle(promptResult);
                titleGenerationRequest.ResponseTask.SetResult(promptResult);
            }
        });
    }

    private void BeforeSubmittingPrompt(Conversation conversation)
    {
        bool conversationIsInitialized = conversation == _lastConversationUsed;

        if (!conversationIsInitialized)
        {
            if (_multiTurnConversation != null)
            {
                _multiTurnConversation.Dispose();
                _multiTurnConversation = null;
            }

            // Latest chat history of this conversation was generated with a different model
            bool lastUsedDifferentModel = LMKitConfig.LoadedModelUri != conversation.LastUsedModelUri;
            bool shouldUseCurrentChatHistory = !lastUsedDifferentModel && conversation.ChatHistory != null;
            bool shouldDeserializeChatHistoryData = (lastUsedDifferentModel && conversation.LatestChatHistoryData != null) || (!lastUsedDifferentModel && conversation.ChatHistory == null);

            if (shouldUseCurrentChatHistory || shouldDeserializeChatHistoryData)
            {
                ChatHistory? chatHistory = shouldUseCurrentChatHistory ? conversation.ChatHistory : ChatHistory.Deserialize(conversation.LatestChatHistoryData, _model);

                _multiTurnConversation = new MultiTurnConversation(_model, chatHistory, LMKitConfig.ContextSize)
                {
                    SamplingMode = GetTokenSampling(LMKitConfig),
                    MaximumCompletionTokens = LMKitConfig.MaximumCompletionTokens,
                };
            }
            else
            {
                _multiTurnConversation = new MultiTurnConversation(_model, LMKitConfig.ContextSize)
                {
                    SamplingMode = GetTokenSampling(LMKitConfig),
                    MaximumCompletionTokens = LMKitConfig.MaximumCompletionTokens,
                    SystemPrompt = LMKitConfig.SystemPrompt
                };
            }
            _multiTurnConversation.AfterTokenSampling += conversation.AfterTokenSampling;
            conversation.ChatHistory = _multiTurnConversation.ChatHistory;
            conversation.LastUsedModelUri = LMKitConfig.LoadedModelUri;
            _lastConversationUsed = conversation;
        }
        else //updating sampling options, if any.
        {
            //todo: Implement a mechanism to determine whether SamplingMode and MaximumCompletionTokens need to be updated.
            _multiTurnConversation!.SamplingMode = GetTokenSampling(LMKitConfig);
            _multiTurnConversation.MaximumCompletionTokens = LMKitConfig.MaximumCompletionTokens;

            if (LMKitConfig.ContextSize != _multiTurnConversation.ContextSize)
            {
                //todo: implement context size update.
            }
        }
    }

    private bool OnModelLoadingProgressed(float progress)
    {
        ModelLoadingProgressed?.Invoke(this, new ModelLoadingProgressedEventArgs(_currentlyLoadingModelUri!, progress));

        return true;
    }

    private static TokenSampling GetTokenSampling(LMKitConfig config)
    {
        switch (config.SamplingMode)
        {
            default:
            case SamplingMode.Random:
                return new RandomSampling()
                {
                    Temperature = config.RandomSamplingConfig.Temperature,
                    DynamicTemperatureRange = config.RandomSamplingConfig.DynamicTemperatureRange,
                    TopP = config.RandomSamplingConfig.TopP,
                    TopK = config.RandomSamplingConfig.TopK,
                    MinP = config.RandomSamplingConfig.MinP,
                    LocallyTypical = config.RandomSamplingConfig.LocallyTypical
                };

            case SamplingMode.Greedy:
                return new GreedyDecoding();

            case SamplingMode.Mirostat2:
                return new Mirostat2Sampling()
                {
                    Temperature = config.Mirostat2SamplingConfig.Temperature,
                    LearningRate = config.Mirostat2SamplingConfig.LearningRate,
                    TargetEntropy = config.Mirostat2SamplingConfig.TargetEntropy
                };
        }
    }
}
