using CommunityToolkit.Mvvm.ComponentModel;
using LMKit.Maestro.Services;

namespace LMKit.Maestro.ViewModels
{
    public partial class TopNSigmaSamplingSettingsViewModel : ObservableObject
    {
        private readonly TopNSigmaSamplingConfig _config;

        public float Temperature
        {
            get => _config.Temperature;
            set
            {
                float newValue = (float)Math.Round(value, 2);

                if (newValue != _config.Temperature)
                {
                    _config.Temperature = newValue;
                    OnPropertyChanged();
                }
            }
        }

        public float TopNSigma
        {
            get => _config.TopNSigma;
            set
            {
                float newValue = (float)Math.Round(value, 2);

                if (newValue != _config.TopNSigma)
                {
                    _config.TopNSigma = newValue;
                    OnPropertyChanged();
                }
            }
        }

        public int TopK
        {
            get => _config.TopK;
            set
            {
                if (_config.TopK != value)
                {
                    _config.TopK = value;
                    OnPropertyChanged();
                }
            }
        }


        public TopNSigmaSamplingSettingsViewModel(TopNSigmaSamplingConfig randomSamplingConfig)
        {
            _config = randomSamplingConfig;
        }

        public void Reset()
        {
            Temperature = LMKitDefaultSettings.DefaultRandomSamplingTemperature;
            TopNSigma = LMKitDefaultSettings.DefaultTopNSigmaSampling;
            TopK = LMKitDefaultSettings.DefaultRandomSamplingTopK;
        }
    }
}
