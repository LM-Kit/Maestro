using CommunityToolkit.Mvvm.ComponentModel;
using LMKit.Maestro.Services;

namespace LMKit.Maestro.ViewModels
{
    public partial class Mirostat2SamplingSettingsViewModel : ObservableObject
    {
        private readonly Mirostat2SamplingConfig _config;

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

        public float TargetEntropy
        {
            get => _config.TargetEntropy;
            set
            {
                if (_config.TargetEntropy != value)
                {
                    _config.TargetEntropy = value;
                    OnPropertyChanged();
                }
            }
        }

        public float LearningRate
        {
            get => _config.LearningRate;
            set
            {
                float newValue = (float)Math.Round(value, 2);

                if (newValue != _config.LearningRate)
                {
                    _config.LearningRate = newValue;
                    OnPropertyChanged();
                }
            }
        }

        public Mirostat2SamplingSettingsViewModel(Mirostat2SamplingConfig Mirostat2SamplingConfig)
        {
            _config = Mirostat2SamplingConfig;
        }

        public void Reset()
        {
            Temperature = LMKitDefaultSettings.DefaultMirostat2SamplingTemperature;
            TargetEntropy = LMKitDefaultSettings.DefaultMirostat2SamplingTargetEntropy;
            LearningRate = LMKitDefaultSettings.DefaultMirostat2SamplingLearningRate;
        }
    }
}
