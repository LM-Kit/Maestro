using LMKit.Model;

namespace LMKit.Maestro.Services;

public partial class LLMFileManager
{
    public class ModelDownloadingProgressedEventArgs : EventArgs
    {
        public ModelCard ModelCard { get; }

        public long BytesRead { get; }

        public long? ContentLength { get; }

        public double Progress { get; }

        public ModelDownloadingProgressedEventArgs(ModelCard modelCard, long bytesRead, long? contentLength, double progress)
        {
            ModelCard = modelCard;
            BytesRead = bytesRead;
            ContentLength = contentLength;
            Progress = progress;
        }
    }
}
