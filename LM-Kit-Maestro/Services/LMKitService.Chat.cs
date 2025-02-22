using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using LMKit.TextGeneration;
using LMKit.TextGeneration.Chat;
using System.ComponentModel;

namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    public partial class LMKitChat : INotifyPropertyChanged
    {
        private readonly SemaphoreSlim _lmKitServiceSemaphore = new SemaphoreSlim(1);
        private readonly RequestSchedule _titleGenerationSchedule = new RequestSchedule();
        private readonly RequestSchedule _requestSchedule = new RequestSchedule();

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly LMKitServiceState _state;

        private MultiTurnConversation? _multiTurnConversation;
        private Conversation? _lastConversationUsed;

        public LMKitChat(LMKitServiceState state)
        {
            _state = state;
        }

        public async Task<LMKitResult> SubmitPrompt(Conversation conversation, string prompt)
        {
            var promptRequest = new ChatRequest(conversation, ChatRequest.ChatRequestType.Prompt,
                prompt, _state.Config.RequestTimeout);

            ScheduleRequest(promptRequest);

            return await HandlePrompt(promptRequest);
        }

        public void TerminateChatService()
        {
            if (_requestSchedule.Count > 0)
            {
                if (_requestSchedule.RunningPromptRequest != null)
                {
                    _requestSchedule.RunningPromptRequest.CancelAndAwaitTermination();
                }

                while (_requestSchedule.Next != null)
                {
                    _requestSchedule.Next!.CancelAndAwaitTermination();
                }
            }

            if (_titleGenerationSchedule.RunningPromptRequest != null && !_titleGenerationSchedule.RunningPromptRequest.CancellationTokenSource.IsCancellationRequested)
            {
                _titleGenerationSchedule.RunningPromptRequest.CancelAndAwaitTermination();
            }
            else if (_titleGenerationSchedule.Count > 1)
            {
                _titleGenerationSchedule.Next!.CancelAndAwaitTermination();
            }

            if (_multiTurnConversation != null)
            {
                _multiTurnConversation.Dispose();
                _multiTurnConversation = null;
            }
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

        public async Task<LMKitResult> RegenerateResponse(Conversation conversation, ChatHistory.Message message)
        {
            var regenerateResponseRequest = new ChatRequest(conversation,
                ChatRequest.ChatRequestType.RegenerateResponse,
                message, _state.Config.RequestTimeout);

            ScheduleRequest(regenerateResponseRequest);

            return await HandlePrompt(regenerateResponseRequest);
        }

        private void ScheduleRequest(ChatRequest request)
        {
            _requestSchedule.Schedule(request);

            if (_requestSchedule.Count > 1)
            {
                request.CanBeExecutedSignal.WaitOne();
            }
        }

        private async Task<LMKitResult> HandlePrompt(ChatRequest request)
        {
            LMKitResult result;

            try
            {
                _lmKitServiceSemaphore.Wait();

                if (request.CancellationTokenSource.IsCancellationRequested)
                {
                    result = new LMKitResult()
                    {
                        Status = LMKitRequestStatus.Cancelled
                    };
                }
                else
                {
                    BeforeSubmittingPrompt(request.Conversation);

                    try
                    {
                        result = await SubmitPrompt(request);
                    }
                    finally
                    {
                        AfterSubmittingPrompt(request.Conversation);
                    }
                }

                request.ResponseTask.TrySetResult(result);
            }
            catch (Exception exception)
            {
                result = new LMKitResult()
                {
                    Exception = exception,
                    Status = LMKitRequestStatus.GenericError
                };
            }
            finally
            {
                _lmKitServiceSemaphore.Release();

                if (_requestSchedule.Contains(request))
                {
                    _requestSchedule.Remove(request);
                }
            }

            return result;
        }

        private async Task<LMKitResult> SubmitPrompt(ChatRequest request)
        {
            try
            {
                _requestSchedule.RunningPromptRequest = request;
                _lmKitServiceSemaphore.Release();

                var result = new LMKitResult();

                try
                {
                    if (request.RequestType == ChatRequest.ChatRequestType.Prompt)
                    {
                        result.Result = await _multiTurnConversation!.SubmitAsync((string)request.Parameters!, request.CancellationTokenSource.Token);
                    }
                    else if (request.RequestType == ChatRequest.ChatRequestType.RegenerateResponse)
                    {
                        result.Result = await _multiTurnConversation!.RegenerateResponseAsync(request.CancellationTokenSource.Token);
                    }
                }
                catch (Exception exception)
                {
                    result.Exception = exception;

                    if (result.Exception is OperationCanceledException)
                    {
                        result.Status = LMKitRequestStatus.Cancelled;
                    }
                    else
                    {
                        result.Status = LMKitRequestStatus.GenericError;
                    }
                }

                request.Conversation.ChatHistory = _multiTurnConversation!.ChatHistory;
                request.Conversation.LatestChatHistoryData = _multiTurnConversation.ChatHistory.Serialize();

                if (request.Conversation.GeneratedTitleSummary == null &&
                    result.Status == LMKitRequestStatus.OK &&
                    !string.IsNullOrEmpty(((TextGenerationResult)result.Result!).Completion))
                {
                    GenerateConversationSummaryTitle(request.Conversation);
                }

                return result;
            }
            catch (Exception exception)
            {
                return new LMKitResult()
                {
                    Exception = exception,
                    Status = LMKitRequestStatus.GenericError
                };
            }
            finally
            {
                _requestSchedule.RunningPromptRequest = null;
            }
        }

        private void GenerateConversationSummaryTitle(Conversation conversation)
        {
            string firstMessage = conversation.ChatHistory!.Messages.First(message => message.AuthorRole == AuthorRole.User).Content;
            ChatRequest titleGenerationRequest = new ChatRequest(conversation, ChatRequest.ChatRequestType.GenerateTitle, firstMessage, 60);

            _titleGenerationSchedule.Schedule(titleGenerationRequest);

            if (_titleGenerationSchedule.Count > 1)
            {
                titleGenerationRequest.CanBeExecutedSignal.WaitOne();
            }

            _titleGenerationSchedule.RunningPromptRequest = titleGenerationRequest;

            Task.Run(async () =>
            {
                Summarizer summarizer = new Summarizer(_state.LoadedModel)
                {
                    MaximumContextLength = 512,
                    GenerateContent = false,
                    GenerateTitle = true,
                    MaxTitleWords = 10,
                    Guidance = "This content corresponds to the initial user message in a multi-turn conversation",
                    OverflowStrategy = Summarizer.OverflowResolutionStrategy.Truncate
                };

                LMKitResult promptResult = new LMKitResult();

                try
                {
                    promptResult.Result = await summarizer.SummarizeAsync(firstMessage, titleGenerationRequest.CancellationTokenSource.Token);
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
            if (conversation != _lastConversationUsed &&
                _multiTurnConversation != null)
            {
                _multiTurnConversation.AfterTokenSampling -= conversation.AfterTokenSampling;
                _multiTurnConversation.Dispose();
                _multiTurnConversation = null;
            }

            if (_multiTurnConversation == null)
            {
                // Latest chat history of this conversation was generated with a different model
                bool lastUsedDifferentModel = _state.Config.LoadedModelUri != conversation.LastUsedModelUri;
                bool shouldUseCurrentChatHistory = !lastUsedDifferentModel && conversation.ChatHistory != null;
                bool shouldDeserializeChatHistoryData = (lastUsedDifferentModel && conversation.LatestChatHistoryData != null) || (!lastUsedDifferentModel && conversation.ChatHistory == null);

                if (shouldUseCurrentChatHistory)
                {
                    _multiTurnConversation = new MultiTurnConversation(_state.LoadedModel, conversation.ChatHistory, _state.Config.ContextSize);
                }
                else if (shouldDeserializeChatHistoryData)
                {
                    var chatHistory = ChatHistory.Deserialize(conversation.LatestChatHistoryData, _state.LoadedModel);
                    _multiTurnConversation = new MultiTurnConversation(_state.LoadedModel, chatHistory, _state.Config.ContextSize);
                }
                else
                {
                    _multiTurnConversation = new MultiTurnConversation(_state.LoadedModel, _state.Config.ContextSize);
                }

                _multiTurnConversation.AfterTokenSampling += conversation.AfterTokenSampling;
            }

            // Binding everything
            conversation.ChatHistory = _multiTurnConversation.ChatHistory;
            conversation.LastUsedModelUri = _state.Config.LoadedModelUri;
            _lastConversationUsed = conversation;

            // Update conversation
            if (_multiTurnConversation.ChatHistory.MessageCount == 0 ||
                _multiTurnConversation.ChatHistory.Messages.Last().AuthorRole == AuthorRole.System)
            {
                _multiTurnConversation.SystemPrompt = _state.Config.SystemPrompt;
            }

            _multiTurnConversation.SamplingMode = GetTokenSampling(_state.Config);
            _multiTurnConversation.MaximumCompletionTokens = _state.Config.MaximumCompletionTokens;
            //

            conversation.InTextCompletion = true;
        }

        private void AfterSubmittingPrompt(Conversation conversation)
        {
            conversation.InTextCompletion = false;
        }
    }
}