using CommunityToolkit.Mvvm.ComponentModel;

namespace LMKit.Maestro.ViewModels
{
    public partial class DownloadInfo : ObservableObject
    {
        [ObservableProperty]
        long _bytesRead;

        [ObservableProperty]
        long? _contentLength;

        [ObservableProperty]
        double _progress;

        [ObservableProperty]
        bool _isDownloading;

        [ObservableProperty]
        bool _isDownloadPaused;

        [ObservableProperty]
        DownloadTerminationReason? _terminationReason;

        [ObservableProperty]
        bool _downloadCompleted;
    }

    public partial class DownloadInfo : ObservableObject
    {
        public enum DownloadTerminationReason
        {
            DownloadCompleted,
            DownloadCanceled,
            DownloadError
        }
    }
}
