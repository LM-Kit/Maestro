namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    private sealed partial class LMKitRequest
    {
        public enum LMKitRequestType
        {
            Prompt,
            RegenerateResponse,
            GenerateTitle,
        }
    }
}
