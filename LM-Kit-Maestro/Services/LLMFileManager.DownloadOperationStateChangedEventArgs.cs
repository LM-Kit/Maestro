namespace LMKit.Maestro.Services;

public partial class LLMFileManager
{
    public class DownloadOperationStateChangedEventArgs : EventArgs
    {
        public Uri DownloadUrl { get; }

        public DownloadOperationStateChangedType Type { get; }

        public long BytesRead { get; }

        public long? ContentLength { get; }

        public double Progress { get; }

        public Exception? Exception { get; }

        public enum DownloadOperationStateChangedType
        {
            Started,
            Paused,
            Canceled,
            Resumed,
            Progressed,
            Completed
        }

        public DownloadOperationStateChangedEventArgs(Uri downloadUrl, DownloadOperationStateChangedType type)
        {
            DownloadUrl = downloadUrl;
            Type = type;
        }

        public DownloadOperationStateChangedEventArgs(Uri downloadUrl, DownloadOperationStateChangedType type, long bytesRead, long? contentLength, double progress, Exception? exception) : this(downloadUrl, type)
        {
            BytesRead = bytesRead;
            ContentLength = contentLength;
            Progress = progress;
            Exception = exception;
        }

        public DownloadOperationStateChangedEventArgs(Uri downloadUrl, long bytesRead, long? contentLength, double progress) : this(downloadUrl, DownloadOperationStateChangedType.Progressed)
        {
            Type = DownloadOperationStateChangedType.Progressed;
            BytesRead = bytesRead;
            ContentLength = contentLength;
            Progress = progress;
        }

        public DownloadOperationStateChangedEventArgs(Uri downloadUrl, Exception? exception) : this(downloadUrl, DownloadOperationStateChangedType.Completed)
        {
            Exception = exception;
        }
    }
}