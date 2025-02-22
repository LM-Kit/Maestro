using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Services;

namespace LMKit.Maestro.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly IAppSettingsService _appSettingsService;
        private readonly LMKitConfig _config;

        public string SystemPrompt
        {
            get => _config.SystemPrompt;
            set
            {
                if (_config.SystemPrompt != value)
                {
                    _config.SystemPrompt = value;
                    OnPropertyChanged();
                }
            }
        }

        public int MaximumCompletionTokens
        {
            get => _config.MaximumCompletionTokens;
            set
            {
                if (_config.MaximumCompletionTokens != value)
                {
                    _config.MaximumCompletionTokens = value;
                    OnPropertyChanged();
                }
            }
        }

        public int RequestTimeout
        {
            get => _config.RequestTimeout;
            set
            {
                if (_config.RequestTimeout != value)
                {
                    _config.RequestTimeout = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ContextSize
        {
            get => _config.ContextSize;
            set
            {
                int newValue = (int)Math.Round(value / 128.0) * 128;

                if (_config.ContextSize != newValue)
                {
                    _config.ContextSize = newValue;
                    OnPropertyChanged();
                }
            }
        }

        public SamplingMode SamplingMode
        {
            get => _config.SamplingMode;
            set
            {
                if (_config.SamplingMode != value)
                {
                    _config.SamplingMode = value;
                    OnPropertyChanged();
                }
            }
        }

        [ObservableProperty]
        RandomSamplingSettingsViewModel _randomSamplingSettings;

        [ObservableProperty]
        TopNSigmaSamplingSettingsViewModel _topNSigmaSamplingSettings;

        [ObservableProperty]
        Mirostat2SamplingSettingsViewModel _Mirostat2SamplingSettings;

        public SettingsViewModel(IAppSettingsService appSettingsService, LMKitService lmkitService)
        {
            _appSettingsService = appSettingsService;
            _config = lmkitService.LMKitConfig;
            RandomSamplingSettings = new RandomSamplingSettingsViewModel(_config.RandomSamplingConfig);
            Mirostat2SamplingSettings = new Mirostat2SamplingSettingsViewModel(_config.Mirostat2SamplingConfig);
            TopNSigmaSamplingSettings = new TopNSigmaSamplingSettingsViewModel(_config.TopNSigmaSamplingConfig);
        }

        [RelayCommand]
        public void ResetDefaultValues()
        {
            SystemPrompt = LMKitDefaultSettings.DefaultSystemPrompt;
            SamplingMode = LMKitDefaultSettings.DefaultSamplingMode;
            MaximumCompletionTokens = LMKitDefaultSettings.DefaultMaximumCompletionTokens;
            RequestTimeout = LMKitDefaultSettings.DefaultRequestTimeout;
            ContextSize = LMKitDefaultSettings.DefaultContextSize;
            RandomSamplingSettings.Reset();
            Mirostat2SamplingSettings.Reset();
            TopNSigmaSamplingSettings.Reset();
        }

        public void Init()
        {
            SystemPrompt = _appSettingsService.SystemPrompt;
            SamplingMode = _appSettingsService.SamplingMode;
            MaximumCompletionTokens = _appSettingsService.MaximumCompletionTokens;
            RequestTimeout = _appSettingsService.RequestTimeout;
            SamplingMode = _appSettingsService.SamplingMode;
            ContextSize = _appSettingsService.ContextSize;

            var randomSamplingConfig = _appSettingsService.RandomSamplingConfig;
            RandomSamplingSettings.Temperature = randomSamplingConfig.Temperature;
            RandomSamplingSettings.DynamicTemperatureRange = randomSamplingConfig.DynamicTemperatureRange;
            RandomSamplingSettings.TopP = randomSamplingConfig.TopP;
            RandomSamplingSettings.MinP = randomSamplingConfig.MinP;
            RandomSamplingSettings.TopK = randomSamplingConfig.TopK;
            RandomSamplingSettings.LocallyTypical = randomSamplingConfig.LocallyTypical;

            var topNSigmaConfig = _appSettingsService.TopNSigmaSamplingConfig;
            TopNSigmaSamplingSettings.Temperature = topNSigmaConfig.Temperature;
            TopNSigmaSamplingSettings.TopK = topNSigmaConfig.TopK;
            TopNSigmaSamplingSettings.TopNSigma = topNSigmaConfig.TopNSigma;

            var Mirostat2SamplingConfig = _appSettingsService.Mirostat2SamplingConfig;
            Mirostat2SamplingSettings.Temperature = Mirostat2SamplingConfig.Temperature;
            Mirostat2SamplingSettings.TargetEntropy = Mirostat2SamplingConfig.TargetEntropy;
            Mirostat2SamplingSettings.LearningRate = Mirostat2SamplingConfig.LearningRate;
        }

        public void Save()
        {
            _appSettingsService.LastLoadedModelUri = _config.LoadedModelUri;
            _appSettingsService.SystemPrompt = _config.SystemPrompt;
            _appSettingsService.MaximumCompletionTokens = _config.MaximumCompletionTokens;
            _appSettingsService.RequestTimeout = _config.RequestTimeout;
            _appSettingsService.ContextSize = _config.ContextSize;
            _appSettingsService.SamplingMode = _config.SamplingMode;
            _appSettingsService.RandomSamplingConfig = _config.RandomSamplingConfig;
            _appSettingsService.Mirostat2SamplingConfig = _config.Mirostat2SamplingConfig;
            _appSettingsService.TopNSigmaSamplingConfig = _config.TopNSigmaSamplingConfig;
        }

        public void ResetSystemPrompt()
        {
            SystemPrompt = LMKitDefaultSettings.DefaultSystemPrompt;
        }
    }
}
