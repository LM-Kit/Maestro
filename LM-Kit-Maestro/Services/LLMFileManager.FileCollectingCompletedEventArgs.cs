namespace LMKit.Maestro.Services;

public partial class LLMFileManager
{
    public class FileCollectingCompletedEventArgs : EventArgs
    {
        public bool Success { get; }

        public Exception? Exception { get; }

        public FileCollectingCompletedEventArgs(bool success, Exception? exception)
        {
            Success = success;
            Exception = exception;
        }
    }
}
