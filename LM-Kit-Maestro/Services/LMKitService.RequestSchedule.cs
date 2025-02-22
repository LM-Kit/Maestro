namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    private sealed class RequestSchedule
    {
        private readonly object _locker = new object();

        private List<ChatRequest> _scheduledPrompts = new List<ChatRequest>();

        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return _scheduledPrompts.Count;
                }
            }
        }

        public ChatRequest? Next
        {
            get
            {
                lock (_locker)
                {
                    if (_scheduledPrompts.Count > 0)
                    {
                        var scheduledPrompt = _scheduledPrompts[0];

                        return scheduledPrompt;
                    }

                    return null;
                }
            }
        }

        public ChatRequest? RunningPromptRequest { get; set; }

        public void Schedule(ChatRequest promptRequest)
        {
            lock (_locker)
            {
                _scheduledPrompts.Add(promptRequest);

                if (Count == 1)
                {
                    promptRequest.CanBeExecutedSignal.Set();
                }
            }
        }

        public bool Contains(ChatRequest scheduledPrompt)
        {
            lock (_locker)
            {
                return _scheduledPrompts.Contains(scheduledPrompt);
            }
        }

        public void Remove(ChatRequest scheduledPrompt)
        {
            lock (_locker)
            {
                HandleScheduledPromptRemoval(scheduledPrompt);
            }
        }

        public ChatRequest? Unschedule(Conversation conversation)
        {
            ChatRequest? prompt = null;

            lock (_locker)
            {
                foreach (var scheduledPrompt in _scheduledPrompts)
                {
                    if (scheduledPrompt.Conversation == conversation)
                    {
                        prompt = scheduledPrompt;
                        break;
                    }
                }

                if (prompt != null)
                {
                    HandleScheduledPromptRemoval(prompt);
                }
            }

            return prompt;
        }

        private void HandleScheduledPromptRemoval(ChatRequest scheduledPrompt)
        {
            bool wasFirstInLine = scheduledPrompt == _scheduledPrompts[0];

            _scheduledPrompts.Remove(scheduledPrompt);

            if (wasFirstInLine && Next != null)
            {
                Next.CanBeExecutedSignal.Set();
            }
            else
            {
                scheduledPrompt.CanBeExecutedSignal.Set();
            }
        }
    }
}
