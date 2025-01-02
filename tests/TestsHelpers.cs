using LMKit.Maestro.Services;
using Maestro.Tests.Services;
using LMKit.TextGeneration.Chat;
using System.Reflection;
using System.Text;

namespace Maestro.Tests;

internal static class TestsHelpers
{
    public static void AssertPromptResponseIsSuccessful(LMKitService.LMKitResult promptResult)
    {
        Assert.Equal(LMKitRequestStatus.OK, promptResult.Status);
        Assert.Null(promptResult.Exception);
        Assert.NotNull(promptResult.Result);
    }

    public static void AssertConversationPromptSuccessState(ConversationViewModelWrapper testConversation, int expectedMessageCount = 2)
    {
        Assert.True(testConversation.PromptResultTask.Task.Result);
        Assert.True(testConversation.ConversationViewModel.Messages.Count == expectedMessageCount);
        Assert.Equal(LMKitRequestStatus.OK, testConversation.ConversationViewModel.Messages[0].Status);
        Assert.Equal(LMKitRequestStatus.OK, testConversation.ConversationViewModel.Messages[1].Status);
        Assert.False(string.IsNullOrEmpty(testConversation.ConversationViewModel.Messages[0].Content));
        Assert.False(string.IsNullOrEmpty(testConversation.ConversationViewModel.Messages[1].Content));
        Assert.False(testConversation.ConversationViewModel.AwaitingResponse);
        Assert.False(testConversation.ConversationViewModel.Messages[1].MessageInProgress);
    }

    public static void AssertConversationPromptCancelledState(ConversationViewModelWrapper testConversation)
    {
        Assert.False(testConversation.PromptResultTask.Task.Result);

        if (testConversation.ConversationViewModel.Messages.Count == 2)
        {
            Assert.Equal(LMKitRequestStatus.Cancelled, testConversation.ConversationViewModel.Messages[1].Status);
            Assert.False(string.IsNullOrEmpty(testConversation.ConversationViewModel.Messages[0].Content));

        }

        Assert.False(testConversation.ConversationViewModel.AwaitingResponse);
    }

    public static int CountUserAndAssistantMessages(ChatHistory chatHistory)
    {
        int count = 0;

        foreach (var message in chatHistory.Messages)
        {
            if (message.AuthorRole == AuthorRole.Assistant || message.AuthorRole == AuthorRole.User)
            {
                count++;
            }
        }

        return count;
    }

    public static byte[] GetTestChatHistoryData()
    {
        var assembly = Assembly.GetExecutingAssembly();

        using (Stream chatHistoryStream = assembly!.GetManifestResourceStream("Maestro.Tests.Resources.chat-history.txt")!)
        using (StreamReader reader = new(chatHistoryStream))
        {
            string result = reader.ReadToEnd();

            return Encoding.UTF8.GetBytes(result);
        }
    }
}
