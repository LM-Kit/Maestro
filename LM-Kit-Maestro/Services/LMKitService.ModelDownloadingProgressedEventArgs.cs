namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    public sealed class ModelDownloadingProgressedEventArgs : NotifyModelStateChangedEventArgs
    {
        public string Path { get; }
        public long? ContentLength { get; }
        public long BytesRead { get; }

        public ModelDownloadingProgressedEventArgs(Uri fileUri, string path, long? contentLength, long bytesRead) : base(fileUri)
        {
            Path = path;
            ContentLength = contentLength;
            BytesRead = bytesRead;
        }
    }
}



