using LMKit.TextGeneration;
using System.ComponentModel;

namespace LMKit.Maestro.Services;

public partial class LMKitService : INotifyPropertyChanged
{
    private sealed class LMKitRequest
    {
        public ManualResetEvent CanBeExecutedSignal { get; } = new ManualResetEvent(false);
        public CancellationTokenSource CancellationTokenSource { get; }
        public TaskCompletionSource<LMKitResult> ResponseTask { get; } = new TaskCompletionSource<LMKitResult>();
        public object? Parameters { get; }

        public LMKitRequestType RequestType { get; }

        public LMKitRequest(LMKitRequestType requestType, object? parameter, int requestTimeout)
        {
            RequestType = requestType;
            Parameters = parameter;
            CancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(requestTimeout));
        }

        public void CancelAndAwaitTermination()
        {
            CancellationTokenSource.Cancel();
            ResponseTask.Task.Wait();
        }

        public enum LMKitRequestType
        {
            Prompt,
            RegenerateResponse,
            GenerateTitle,
            Translate
        }

        public sealed class PromptRequestParameters
        {
            public Conversation Conversation { get; set; }

            public string Prompt { get; set; }

            public PromptRequestParameters(Conversation conversation, string prompt)
            {
                Conversation = conversation;
                Prompt = prompt;
            }
        }

        public sealed class TranslationRequestParameters
        {
            public string InputText { get; set; }

            public Language Language { get; set; }

            public TranslationRequestParameters(string inputText, Language language)
            {
                InputText = inputText;
                Language = language;
            }
        }
    }
}
