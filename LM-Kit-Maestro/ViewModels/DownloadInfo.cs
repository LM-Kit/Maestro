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

        private bool _isDownloading;
        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                if (_isDownloading != value)
                {
                    _isDownloading = value;
                    OnPropertyChanged(nameof(IsDownloading));
                }
            }
        }

        private bool _isDownloadPaused;
        public bool IsDownloadPaused
        {
            get => _isDownloadPaused;
            set
            {
                if (_isDownloadPaused != value)
                {
                    _isDownloading = value;
                    OnPropertyChanged(nameof(IsDownloadPaused));
                }
            }
        }
    }
}
