namespace LMKit.Maestro.Services;

public static class LMKitDefaultSettings
{
    public static readonly string DefaultModelStorageDirectory = Global.Configuration.ModelStorageDirectory;
    public static readonly string DefaultChatHistoryDirectory = FileSystem.AppDataDirectory;

    public const string DefaultSystemPrompt = "You are Maestro, a chatbot designed to provide prompt, helpful, and accurate responses to user requests in a friendly and professional manner.";
    public const int DefaultMaximumCompletionTokens = 2048; // TODO: Evan, consider setting this to -1 to indicate no limitation. Ensure the option to configure the chat with a predefined limit remains available.
    public static readonly int DefaultContextSize = Hardware.DeviceConfiguration.GetOptimalContextSize();
    public const int DefaultRequestTimeout = 120;
    public const SamplingMode DefaultSamplingMode = SamplingMode.Random;

    public const bool DefaultEnableLowPerformanceModels = false;
    public const bool DefaultEnablePredefinedModels = true;
    public const bool DefaultEnableCustomModels = true;

    public static SamplingMode[] AvailableSamplingModes { get; } = (SamplingMode[])Enum.GetValues(typeof(SamplingMode));

    public const float DefaultRandomSamplingTemperature = 0.8f;
    public const float DefaultRandomSamplingDynamicTemperatureRange = 0f;
    public const float DefaultRandomSamplingTopP = 0.95f;
    public const float DefaultRandomSamplingMinP = 0.05f;
    public const int DefaultRandomSamplingTopK = 40;
    // public const float DefaultRandomSamplingLocallyTypical = 1;

    public const float DefaultTopNSigmaSampling = 1f;

    public const float DefaultMirostat2SamplingTemperature = 0.8f;
    public const float DefaultMirostat2SamplingTargetEntropy = 5.0f;
    public const float DefaultMirostat2SamplingLearningRate = 0.1f;

}
