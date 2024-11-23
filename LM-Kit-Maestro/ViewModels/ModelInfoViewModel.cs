using LMKit.Maestro.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LMKit.Maestro.ViewModels
{
    public partial class ModelInfoViewModel : ViewModelBase
    {
        private ModelInfo _modelInfo;
        public ModelInfo ModelInfo
        {
            get => _modelInfo;
            set
            {
                _modelInfo = value;
                Name = _modelInfo.FileName;
                FileSize = _modelInfo.FileSize ?? 0;
                OnPropertyChanged();
            }
        }

        [ObservableProperty]
        long _fileSize;

        [ObservableProperty]
        string _name;

        [ObservableProperty]
        DownloadInfo _downloadInfo = new DownloadInfo();

        public ModelInfoViewModel(ModelInfo modelInfo)
        {
            _modelInfo = modelInfo;
            Name = modelInfo.FileName;
            FileSize = modelInfo.FileSize ?? 0;
        }
    }

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

    public enum DownloadStatus
    {
        NotDownloaded,
        Downloaded,
        Downloading,
        DownloadPaused,
    }
}
