using LMKitMaestro.Services;
using LMKitMaestro.Tests.Services;
using System.Reflection;
using System.Text;

namespace LMKitMaestro.Tests
{
    internal static class LMKitMaestroTestsHelpers
    {
        public static void AssertPromptResponseIsSuccessful(LMKitService.PromptResult promptResult)
        {
            Assert.Equal(promptResult.Status, LmKitTextGenerationStatus.Undefined);
            Assert.Null(promptResult.Exception);
            Assert.NotNull(promptResult.TextGenerationResult);
        }

        public static void AssertConversationPromptSuccessState(ConversationViewModelWrapper testConversation, int expectedMessageCount = 2)
        {
            Assert.True(testConversation.PromptResultTask.Task.Result);
            Assert.True(testConversation.ConversationViewModel.Messages.Count == expectedMessageCount);
            Assert.Equal(LmKitTextGenerationStatus.Undefined, testConversation.ConversationViewModel.Messages[0].Status);
            Assert.Equal(LmKitTextGenerationStatus.Undefined, testConversation.ConversationViewModel.Messages[1].Status);
            Assert.False(string.IsNullOrEmpty(testConversation.ConversationViewModel.Messages[0].Text));
            Assert.False(string.IsNullOrEmpty(testConversation.ConversationViewModel.Messages[1].Text));
            Assert.False(testConversation.ConversationViewModel.AwaitingResponse);
            Assert.False(testConversation.ConversationViewModel.Messages[1].MessageInProgress);
        }

        public static void AssertConversationPromptCancelledState(ConversationViewModelWrapper testConversation)
        {
            Assert.False(testConversation.PromptResultTask.Task.Result);
            Assert.Equal(2, testConversation.ConversationViewModel.Messages.Count);
            Assert.Equal(LmKitTextGenerationStatus.Cancelled, testConversation.ConversationViewModel.Messages[0].Status);
            Assert.Equal(LmKitTextGenerationStatus.Cancelled, testConversation.ConversationViewModel.Messages[1].Status);
            Assert.False(string.IsNullOrEmpty(testConversation.ConversationViewModel.Messages[0].Text));
            //Assert.False(string.IsNullOrEmpty(testConversation.ConversationViewModel.Messages[1].Text));
            Assert.False(testConversation.ConversationViewModel.AwaitingResponse);
            //Assert.False(testConversation.ConversationViewModel.Messages[1].MessageInProgress);
        }

        public static byte[] GetTestChatHistoryData()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream chatHistoryStream = assembly!.GetManifestResourceStream("LMKitMaestroTests.ChatHistorySerialized.txt")!)
            using (StreamReader reader = new(chatHistoryStream))
            {
                string result = reader.ReadToEnd();

                return Encoding.UTF8.GetBytes(result);
            }
        }
    }
}
