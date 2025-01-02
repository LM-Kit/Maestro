using LMKit.Maestro.Services;

namespace LMKit.Maestro.Tests.Services
{
    internal class LMKitDummyConversation
    {
        LMKitService.Conversation Conversation { get; }

        public TaskCompletionSource<LMKitService.LMKitResult?> PromptResultTask { get; private set; } = new TaskCompletionSource<LMKitService.LMKitResult?>();

        public LMKitDummyConversation(LMKitService lmKitService)
        {
            Conversation = new LMKitService.Conversation(lmKitService);
        }

        public async Task SubmitPrompt(LMKitService lmKitService, string prompt)
        {
            LMKitService.LMKitResult? promptResult = null;

            _ = Task.Run(async () =>
            {
                try
                {
                    promptResult = await lmKitService.Chat.SubmitPrompt(Conversation, prompt);
                    PromptResultTask.SetResult(promptResult);
                }
                catch (Exception)
                {
                    PromptResultTask.SetResult(null);
                }
            });

            await Task.Delay(50);
        }
    }
}
