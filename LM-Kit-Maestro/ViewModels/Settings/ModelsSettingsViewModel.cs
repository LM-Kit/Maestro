using LMKit.Maestro.Services;
using System.ComponentModel;

namespace LMKit.Maestro.ViewModels
{
    public partial class ModelsSettingsViewModel : ViewModelBase
    {
        private readonly IAppSettingsService _appSettingsService;

        private readonly LLMFileManagerConfig _config;
        private CancellationTokenSource _debounceCts = new();

        public bool EnableLowPerformanceModels
        {
            get => _config.EnableLowPerformanceModels;
            set
            {
                if (_config.EnableLowPerformanceModels != value)
                {
                    _config.EnableLowPerformanceModels = value;
                    DebounceSave(nameof(EnableLowPerformanceModels), value);
                    OnPropertyChanged();
                }
            }
        }

        public bool EnablePredefinedModels
        {
            get => _config.EnablePredefinedModels;
            set
            {
                if (_config.EnablePredefinedModels != value)
                {
                    _config.EnablePredefinedModels = value;
                    DebounceSave(nameof(EnablePredefinedModels), value);
                    OnPropertyChanged();
                }
            }
        }

        public ModelsSettingsViewModel(IAppSettingsService appSettingsService, ILLMFileManager fileManager)
        {
            _config = fileManager.Config;
            _appSettingsService = appSettingsService;
        }

        private void DebounceSave(string propertyName, bool value)
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500, token);

                    if (!token.IsCancellationRequested)
                    {
                        SaveToSettings(propertyName, value);
                    }
                }
                catch (TaskCanceledException) { }
            }, token);
        }

        private void SaveToSettings(string propertyName, bool value)
        {
            if (propertyName == nameof(EnableLowPerformanceModels))
            {
                _appSettingsService.EnableLowPerformanceModels = value;
            }
            else if (propertyName == nameof(EnablePredefinedModels))
            {
                _appSettingsService.EnablePredefinedModels = value;
            }
        }
    }
}