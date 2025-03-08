using LMKit.Model;

namespace LMKit.Maestro.Services;

public partial class LLMFileManager
{
    public class DownloadOperationStateChangedEventArgs : EventArgs
    {
        public ModelCard ModelCard { get; }

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

        public DownloadOperationStateChangedEventArgs(ModelCard modelCard, DownloadOperationStateChangedType type)
        {
            ModelCard = modelCard;
            Type = type;
        }

        public DownloadOperationStateChangedEventArgs(ModelCard modelCard, DownloadOperationStateChangedType type, long bytesRead, long? contentLength, double progress, Exception? exception) : this(modelCard, type)
        {
            BytesRead = bytesRead;
            ContentLength = contentLength;
            Progress = progress;
            Exception = exception;
        }

        public DownloadOperationStateChangedEventArgs(ModelCard modelCard, long bytesRead, long? contentLength, double progress) : this(modelCard, DownloadOperationStateChangedType.Progressed)
        {
            Type = DownloadOperationStateChangedType.Progressed;
            BytesRead = bytesRead;
            ContentLength = contentLength;
            Progress = progress;
        }

        public DownloadOperationStateChangedEventArgs(ModelCard modelCard, Exception? exception) : this(modelCard, DownloadOperationStateChangedType.Completed)
        {
            Exception = exception;
        }
    }
}