using LMKitMaestro.Services;
using System.Reflection;
using System.Text;

namespace LMKitMaestroTests
{
    internal static class LMKitMaestroTestsHelpers
    {
        public static void AssertPromptResponseIsSuccessful(LMKitService.PromptResult promptResult)
        {
            Assert.True(promptResult.Status == LmKitTextGenerationStatus.Undefined);
            Assert.True(promptResult.Exception == null);
            Assert.True(promptResult.TextGenerationResult != null);
        }

        public static void AssertConversationPromptSuccessState(ConversationViewModelWrapper testConversation, int expectedMessageCount = 2)
        {
            Assert.True(testConversation.PromptResultTask.Task.Result);
            Assert.True(testConversation.ConversationViewModel.Messages.Count == expectedMessageCount);
            Assert.True(testConversation.ConversationViewModel.Messages[0].Status == LmKitTextGenerationStatus.Undefined);
            Assert.True(testConversation.ConversationViewModel.Messages[1].Status == LmKitTextGenerationStatus.Undefined);
            Assert.False(string.IsNullOrEmpty(testConversation.ConversationViewModel.Messages[0].Text));
            Assert.False(string.IsNullOrEmpty(testConversation.ConversationViewModel.Messages[1].Text));
            Assert.False(testConversation.ConversationViewModel.AwaitingResponse);
            Assert.False(testConversation.ConversationViewModel.Messages[1].MessageInProgress);
        }

        public static void AssertConversationPromptCancelledState(ConversationViewModelWrapper testConversation)
        {
            Assert.False(testConversation.PromptResultTask.Task.Result);
            Assert.True(testConversation.ConversationViewModel.Messages.Count == 2);
            Assert.True(testConversation.ConversationViewModel.Messages[0].Status == LmKitTextGenerationStatus.Cancelled);
            Assert.True(testConversation.ConversationViewModel.Messages[1].Status == LmKitTextGenerationStatus.Cancelled);
            Assert.False(string.IsNullOrEmpty(testConversation.ConversationViewModel.Messages[0].Text));
            //Assert.False(string.IsNullOrEmpty(testConversation.ConversationViewModel.Messages[1].Text));
            Assert.False(testConversation.ConversationViewModel.AwaitingResponse);
            //Assert.False(testConversation.ConversationViewModel.Messages[1].MessageInProgress);
        }

        public static byte[] GetTestChatHistoryData()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream chatHistoryStream = assembly!.GetManifestResourceStream("LMKitMaestroTests.ChatHistorySerialized.txt")!)
            using (StreamReader reader = new StreamReader(chatHistoryStream))
            {
                string result = reader.ReadToEnd();

                return Encoding.UTF8.GetBytes(result);
            }
        }
    }
}
