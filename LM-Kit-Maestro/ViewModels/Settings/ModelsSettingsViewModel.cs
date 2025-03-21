using LMKit.Maestro.Services;

namespace LMKit.Maestro.ViewModels
{
    public partial class ModelsSettingsViewModel : ViewModelBase
    {
        private readonly IAppSettingsService _appSettingsService;

        private readonly LLMFileManagerConfig _config;

        public bool EnableLowPerformanceModels
        {
            get => _config.EnableLowPerformanceModels;
            set
            {
                if (_config.EnableLowPerformanceModels != value)
                {
                    _config.EnableLowPerformanceModels = value;
                    _appSettingsService.EnableLowPerformanceModels = value;
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
                    _appSettingsService.EnablePredefinedModels = value;
                    OnPropertyChanged();
                }
            }
        }

        public ModelsSettingsViewModel(IAppSettingsService appSettingsService, ILLMFileManager fileManager)
        {
            _config = fileManager.Config;
            _appSettingsService = appSettingsService;
        }
    }
}