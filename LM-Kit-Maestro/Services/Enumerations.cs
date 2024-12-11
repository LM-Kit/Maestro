namespace LMKit.Maestro.Services;

public enum SamplingMode
{
    Random,
    Greedy,
    Mirostat2
}

public enum LMKitModelLoadingState
{
    Unloaded,
    Loading,
    Loaded
}

public enum LMKitTextGenerationStatus
{
    Undefined,
    Cancelled,
    GenericError
}