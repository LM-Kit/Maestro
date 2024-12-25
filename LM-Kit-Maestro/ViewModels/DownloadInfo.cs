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
        DownloadStatus _status;
    }
}
