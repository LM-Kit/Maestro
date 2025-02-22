namespace LMKit.Maestro.Services;

public sealed class TopNSigmaSamplingConfig
{
    public float Temperature { get; set; } = LMKitDefaultSettings.DefaultRandomSamplingTemperature;

    public float TopNSigma { get; set; } = LMKitDefaultSettings.DefaultTopNSigmaSampling;

    //public float DynamicTemperatureRange { get; set; } = LMKitDefaultSettings.DefaultRandomSamplingDynamicTemperatureRange;


    public int TopK { get; set; } = LMKitDefaultSettings.DefaultRandomSamplingTopK;
}
