using LMKitMaestro.Services;

namespace LMKitMaestroTests.Services
{
    internal class LmKitDummyConversation
    {
        LMKitService.Conversation Conversation { get; }

        public TaskCompletionSource<LMKitService.PromptResult?> PromptResultTask { get; private set; } = new TaskCompletionSource<LMKitService.PromptResult?>();

        public LmKitDummyConversation(LMKitService lmKitService)
        {
            Conversation = new LMKitService.Conversation(lmKitService);
        }

        public void SubmitPrompt(LMKitService lmKitService, string prompt)
        {
            LMKitService.PromptResult? promptResult = null;

            Task.Run(async () =>
            {
                try
                {
                    promptResult = await lmKitService.SubmitPrompt(Conversation, prompt);
                    PromptResultTask.SetResult(promptResult);
                }
                catch (Exception)
                {
                    PromptResultTask.SetResult(null);
                }
            });
        }
    }
}
