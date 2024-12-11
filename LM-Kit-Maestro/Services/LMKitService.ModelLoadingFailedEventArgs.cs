namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    public sealed class ModelLoadingFailedEventArgs : NotifyModelStateChangedEventArgs
    {
        public Exception Exception { get; }

        public ModelLoadingFailedEventArgs(Uri fileUri, Exception exception) : base(fileUri)
        {
            Exception = exception;
        }
    }
}
