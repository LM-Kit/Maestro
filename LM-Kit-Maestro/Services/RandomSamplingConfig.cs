namespace LMKit.Maestro.Services;

public sealed class RandomSamplingConfig
{
    public float Temperature { get; set; } = LMKitDefaultSettings.DefaultRandomSamplingTemperature;

    public float DynamicTemperatureRange { get; set; } = LMKitDefaultSettings.DefaultRandomSamplingDynamicTemperatureRange;

    public float TopP { get; set; } = LMKitDefaultSettings.DefaultRandomSamplingTopP;

    public float MinP { get; set; } = LMKitDefaultSettings.DefaultRandomSamplingMinP;

    public int TopK { get; set; } = LMKitDefaultSettings.DefaultRandomSamplingTopK;

    public float LocallyTypical { get; set; } = LMKitDefaultSettings.DefaultRandomSamplingLocallyTypical;
}
