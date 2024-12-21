﻿using CommunityToolkit.Mvvm.ComponentModel;
using LMKit.Model;

namespace LMKit.Maestro.ViewModels
{
    public partial class ModelInfoViewModel : ViewModelBase
    {
        private ModelCard _modelCard;
        public ModelCard ModelInfo
        {
            get => _modelCard;
            set
            {
                _modelCard = value;
                Name = _modelCard.ModelName;
                FileSize = _modelCard.FileSize;
                OnPropertyChanged();
            }
        }

        public bool IsLocallyAvailable
        {
            get
            {
                return _modelCard.IsLocallyAvailable;
            }
        }

        [ObservableProperty]
        long _fileSize;

        [ObservableProperty]
        string _name;

        [ObservableProperty]
        string _precision;

        [ObservableProperty]
        DownloadInfo _downloadInfo = new DownloadInfo();

        internal void OnLocalModelRemoved()
        {
            OnPropertyChanged(nameof(IsLocallyAvailable));
        }

        public ModelInfoViewModel(ModelCard modelCard)
        {
            _modelCard = modelCard;
            Name = modelCard.ModelName;
            FileSize = modelCard.FileSize;
            Precision = modelCard.QuantizationPrecision.ToString() + (modelCard.QuantizationPrecision > 1 ? "-bits" : "1-bit");
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
