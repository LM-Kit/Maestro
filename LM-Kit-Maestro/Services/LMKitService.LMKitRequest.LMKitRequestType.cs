namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    private sealed partial class ChatRequest
    {
        public enum ChatRequestType
        {
            Prompt,
            RegenerateResponse,
            GenerateTitle,
        }
    }
}
