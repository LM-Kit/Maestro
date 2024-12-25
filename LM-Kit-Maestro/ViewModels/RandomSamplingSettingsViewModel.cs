using CommunityToolkit.Mvvm.ComponentModel;
using LMKit.Maestro.Services;

namespace LMKit.Maestro.ViewModels
{
    public partial class RandomSamplingSettingsViewModel : ObservableObject
    {
        private readonly RandomSamplingConfig _config;

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

        public float DynamicTemperatureRange
        {
            get => _config.DynamicTemperatureRange;
            set
            {
                float newValue = (float)Math.Round(value, 2);

                if (newValue != _config.DynamicTemperatureRange)
                {
                    _config.DynamicTemperatureRange = newValue;
                    OnPropertyChanged();
                }
            }
        }

        public float TopP
        {
            get => _config.TopP;
            set
            {
                float newValue = (float)Math.Round(value, 2);

                if (newValue != _config.TopP)
                {
                    _config.TopP = newValue;
                    OnPropertyChanged();
                }
            }
        }

        public float MinP
        {
            get => _config.MinP;
            set
            {
                float newValue = (float)Math.Round(value, 2);

                if (newValue != _config.MinP)
                {
                    _config.MinP = newValue;
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

        public float LocallyTypical
        {
            get => _config.LocallyTypical;
            set
            {
                float newValue = (float)Math.Round(value, 2);

                if (newValue != _config.LocallyTypical)
                {
                    _config.LocallyTypical = newValue;
                    OnPropertyChanged();
                }
            }
        }

        public RandomSamplingSettingsViewModel(RandomSamplingConfig randomSamplingConfig)
        {
            _config = randomSamplingConfig;
        }

        public void Reset()
        {
            Temperature = LMKitDefaultSettings.DefaultRandomSamplingTemperature;
            DynamicTemperatureRange = LMKitDefaultSettings.DefaultRandomSamplingDynamicTemperatureRange;
            TopP = LMKitDefaultSettings.DefaultRandomSamplingTopP;
            MinP = LMKitDefaultSettings.DefaultRandomSamplingMinP;
            TopK = LMKitDefaultSettings.DefaultRandomSamplingTopK;
            LocallyTypical = LMKitDefaultSettings.DefaultRandomSamplingLocallyTypical;
        }
    }
}
