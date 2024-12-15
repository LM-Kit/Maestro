using LMKit.TextGeneration.Chat;

namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    private sealed partial class LMKitRequest
    {
        public sealed class RegenerateResponseParameters
        {
            public Conversation Conversation { get; set; }

            public ChatHistory.Message Message { get; set; }

            public RegenerateResponseParameters(Conversation conversation, ChatHistory.Message message)
            {
                Conversation = conversation;
                Message = message;
            }
        }
    }
}
