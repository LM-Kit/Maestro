namespace LMKit.Maestro.Services;

public interface IAppSettingsService
{
    Uri? LastLoadedModelUri { get; set; }
    string ModelStorageDirectory { get; set; }
    string SystemPrompt { get; set; }
    int MaximumCompletionTokens { get; set; }
    int RequestTimeout { get; set; }
    int ContextSize { get; set; }
    SamplingMode SamplingMode { get; set; }
    RandomSamplingConfig RandomSamplingConfig { get; set; }
    Mirostat2SamplingConfig Mirostat2SamplingConfig { get; set; }
    TopNSigmaSamplingConfig TopNSigmaSamplingConfig { get; set; }
    bool EnablePredefinedModels { get; set; }
    bool EnableLowPerformanceModels { get; set; }
}
