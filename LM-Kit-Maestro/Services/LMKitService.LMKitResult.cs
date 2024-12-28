namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    public sealed class LMKitResult
    {
        public Exception? Exception { get; set; }

        public LMKitRequestStatus Status { get; set; }

        public object? Result { get; set; }
    }
}
