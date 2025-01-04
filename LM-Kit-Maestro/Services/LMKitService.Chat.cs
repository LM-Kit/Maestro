﻿using LMKit.Model;
using LMKit.TextGeneration;
using LMKit.TextGeneration.Chat;
using LMKit.Translation;
using System.ComponentModel;
using System.Diagnostics;

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
            var promptRequest = new LMKitRequest(conversation, LMKitRequest.LMKitRequestType.Prompt, prompt, _state.Config.RequestTimeout)
            {
                Conversation = conversation
            };

            ScheduleRequest(promptRequest);

            return await HandlePrompt(promptRequest);
        }

        public void CancelAllPrompts()
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
            var regenerateResponseRequest = new LMKitRequest(conversation,
                LMKitRequest.LMKitRequestType.RegenerateResponse,
                message, _state.Config.RequestTimeout)
            {
                Conversation = conversation
            };

            ScheduleRequest(regenerateResponseRequest);

            return await HandlePrompt(regenerateResponseRequest);
        }

        private void ScheduleRequest(LMKitRequest request)
        {
            _requestSchedule.Schedule(request);

            if (_requestSchedule.Count > 1)
            {
                request.CanBeExecutedSignal.WaitOne();
            }
        }

        private async Task<LMKitResult> HandlePrompt(LMKitRequest request)
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

        private async Task<LMKitResult> SubmitPrompt(LMKitRequest request)
        {
            try
            {
                _requestSchedule.RunningPromptRequest = request;
                _lmKitServiceSemaphore.Release();

                var result = new LMKitResult();

                try
                {
                    if (request.RequestType == LMKitRequest.LMKitRequestType.Prompt)
                    {
                        result.Result = await _multiTurnConversation!.SubmitAsync((string)request.Parameters!, request.CancellationTokenSource.Token);
                    }
                    else if (request.RequestType == LMKitRequest.LMKitRequestType.RegenerateResponse)
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
            LMKitRequest titleGenerationRequest = new LMKitRequest(conversation, LMKitRequest.LMKitRequestType.GenerateTitle, firstMessage, 60);

            _titleGenerationSchedule.Schedule(titleGenerationRequest);

            if (_titleGenerationSchedule.Count > 1)
            {
                titleGenerationRequest.CanBeExecutedSignal.WaitOne();
            }

            _titleGenerationSchedule.RunningPromptRequest = titleGenerationRequest;

            Task.Run(async () =>
            {
                Summarizer summarizer = new Summarizer(_state.Model)
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
            bool conversationIsInitialized = conversation == _lastConversationUsed;

            if (!conversationIsInitialized)
            {
                if (_multiTurnConversation != null)
                {
                    _multiTurnConversation.Dispose();
                    _multiTurnConversation = null;
                }

                // Latest chat history of this conversation was generated with a different model
                bool lastUsedDifferentModel = _state.Config.LoadedModelUri != conversation.LastUsedModelUri;
                bool shouldUseCurrentChatHistory = !lastUsedDifferentModel && conversation.ChatHistory != null;
                bool shouldDeserializeChatHistoryData = (lastUsedDifferentModel && conversation.LatestChatHistoryData != null) || (!lastUsedDifferentModel && conversation.ChatHistory == null);

                if (shouldUseCurrentChatHistory || shouldDeserializeChatHistoryData)
                {
                    ChatHistory? chatHistory = shouldUseCurrentChatHistory ? conversation.ChatHistory : ChatHistory.Deserialize(conversation.LatestChatHistoryData, _state.Model);

                    _multiTurnConversation = new MultiTurnConversation(_state.Model, chatHistory, _state.Config.ContextSize)
                    {
                        SamplingMode = GetTokenSampling(_state.Config),
                        MaximumCompletionTokens = _state.Config.MaximumCompletionTokens,
                    };
                }
                else
                {
                    _multiTurnConversation = new MultiTurnConversation(_state.Model, _state.Config.ContextSize)
                    {
                        SamplingMode = GetTokenSampling(_state.Config),
                        MaximumCompletionTokens = _state.Config.MaximumCompletionTokens,
                        SystemPrompt = _state.Config.SystemPrompt
                    };
                }
                _multiTurnConversation.AfterTokenSampling += conversation.AfterTokenSampling;

                conversation.ChatHistory = _multiTurnConversation.ChatHistory;
                conversation.LastUsedModelUri = _state.Config.LoadedModelUri;
                _lastConversationUsed = conversation;
            }
            else //updating sampling options, if any.
            {
                //todo: Implement a mechanism to determine whether SamplingMode and MaximumCompletionTokens need to be updated.
                _multiTurnConversation!.SamplingMode = GetTokenSampling(_state.Config);
                _multiTurnConversation.MaximumCompletionTokens = _state.Config.MaximumCompletionTokens;

                if (_state.Config.ContextSize != _multiTurnConversation.ContextSize)
                {
                    //todo: implement context size update.
                }
            }

            conversation.InTextCompletion = true;
        }

        private void AfterSubmittingPrompt(Conversation conversation)
        {
            conversation.InTextCompletion = false;
        }
    }
}