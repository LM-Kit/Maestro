namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    private sealed partial class ChatRequest
    {
        public ManualResetEvent CanBeExecutedSignal { get; } = new ManualResetEvent(false);
        public CancellationTokenSource CancellationTokenSource { get; }
        public TaskCompletionSource<LMKitResult> ResponseTask { get; } = new TaskCompletionSource<LMKitResult>();
        public object? Parameters { get; }
        public Conversation Conversation { get; }

        public ChatRequestType RequestType { get; }

        public ChatRequest(Conversation conversation, ChatRequestType requestType, object? parameter, int requestTimeout)
        {
            Conversation = conversation;
            RequestType = requestType;
            Parameters = parameter;
            CancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(requestTimeout));
        }

        public void CancelAndAwaitTermination()
        {
            CancellationTokenSource.Cancel();
            ResponseTask.Task.Wait();
        }
    }
}
