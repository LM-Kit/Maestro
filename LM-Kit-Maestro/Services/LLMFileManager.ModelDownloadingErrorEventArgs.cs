namespace LMKit.Maestro.Services;

public partial class LLMFileManager
{
    public class ModelDownloadingErrorEventArgs : EventArgs
    {
        public Exception? Exception { get; }

        public ModelDownloadingErrorEventArgs(Exception? exception)
        {
            Exception = exception;
        }
    }
}
