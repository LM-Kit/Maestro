namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    public sealed class LMKitResult
    {
        public Exception? Exception { get; set; }

        public LMKitTextGenerationStatus Status { get; set; }

        public object? Result { get; set; }
    }
}
