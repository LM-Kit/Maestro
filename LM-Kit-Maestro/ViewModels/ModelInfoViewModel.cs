using CommunityToolkit.Mvvm.ComponentModel;
using LMKit.Model;

namespace LMKit.Maestro.ViewModels
{
    public partial class ModelInfoViewModel : ViewModelBase
    {
        private ModelCard _modelCard;
        public ModelCard ModelCard
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
        string _shortName;

        [ObservableProperty]
        string _modelPath;

        [ObservableProperty]
        string _precision;

        [ObservableProperty]
        bool _isChatModel;

        [ObservableProperty]
        bool _hasVisionCapability;

        [ObservableProperty]
        bool _isCodeModel;

        [ObservableProperty]
        bool _isMathModel;

        [ObservableProperty]
        string _modelSize;

        [ObservableProperty]
        float _compatibilityLevel;

        [ObservableProperty]
        int _maxContextLengthKB;

        [ObservableProperty]
        DownloadInfo _downloadInfo = new DownloadInfo();

        internal void OnLocalModelRemoved()
        {
            OnPropertyChanged(nameof(IsLocallyAvailable));
        }

        internal void OnLocalModelCreated()
        {
            OnPropertyChanged(nameof(IsLocallyAvailable));
        }

        public ModelInfoViewModel(ModelCard modelCard)
        {
            _modelCard = modelCard;
            Name = modelCard.ModelName;
            ShortName = modelCard.ShortModelName;
            FileSize = modelCard.FileSize;
            Precision = modelCard.QuantizationPrecision.ToString() + (modelCard.QuantizationPrecision > 1 ? "-bits" : "-bit");
            ModelSize = Math.Round((double)modelCard.ParameterCount / 1000000000, 1).ToString().Replace(",", ".") + "B";
            IsChatModel = modelCard.Capabilities.HasFlag(ModelCapabilities.Chat);
            HasVisionCapability = modelCard.Capabilities.HasFlag(ModelCapabilities.Vision);
            IsCodeModel = modelCard.Capabilities.HasFlag(ModelCapabilities.CodeCompletion);
            IsMathModel = modelCard.Capabilities.HasFlag(ModelCapabilities.Math);
            ModelPath = modelCard.IsLocallyAvailable ? modelCard.LocalPath : modelCard.ModelUri.ToString();
            CompatibilityLevel = LMKit.Graphics.DeviceConfiguration.GetPerformanceScore(modelCard);
            MaxContextLengthKB = modelCard.ContextLength / 1024;
        }
    }
}
