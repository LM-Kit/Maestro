using LMKit.Maestro.Services;

namespace Maestro.Tests.Services
{
    public class DummyAppSettingsService : IAppSettingsService
    {
        public Uri? LastLoadedModelUri { get; set; } = null;
        public string ModelStorageDirectory { get; set; } = "C:\\Models";
        public string SystemPrompt { get; set; } = "Default system prompt";
        public int MaximumCompletionTokens { get; set; } = 512;
        public int RequestTimeout { get; set; } = 30;
        public int ContextSize { get; set; } = 2048;
        public SamplingMode SamplingMode { get; set; } = SamplingMode.Random;
        public RandomSamplingConfig RandomSamplingConfig { get; set; } = new RandomSamplingConfig();
        public Mirostat2SamplingConfig Mirostat2SamplingConfig { get; set; } = new Mirostat2SamplingConfig();
        public TopNSigmaSamplingConfig TopNSigmaSamplingConfig { get; set; } = new TopNSigmaSamplingConfig();
        public bool EnablePredefinedModels { get; set; } = true;
        public bool EnableLowPerformanceModels { get; set; } = false;
    }

}
