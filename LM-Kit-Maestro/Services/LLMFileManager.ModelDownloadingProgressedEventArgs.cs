namespace LMKit.Maestro.Services;

public partial class LLMFileManager
{
    public class ModelDownloadingProgressedEventArgs : EventArgs
    {
        public string ModelFilePath { get; }

        public long BytesRead { get; }

        public long? ContentLength { get; }

        public double Progress { get; }

        public ModelDownloadingProgressedEventArgs(string modelFilePath, long bytesRead, long? contentLength, double progress)
        {
            ModelFilePath = modelFilePath;
            BytesRead = bytesRead;
            ContentLength = contentLength;
            Progress = progress;
        }
    }
}
