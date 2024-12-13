using System.ComponentModel;

namespace LMKit.Maestro.Services;

public partial class LMKitService : INotifyPropertyChanged
{
    private sealed partial class LMKitRequest
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
    }
}
