namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    private sealed partial class LMKitRequest
    {
        public sealed class PromptRequestParameters
        {
            public Conversation Conversation { get; set; }

            public string Prompt { get; set; }

            public PromptRequestParameters(Conversation conversation, string prompt)
            {
                Conversation = conversation;
                Prompt = prompt;
            }
        }
    }
}
