using LMKitMaestro.Models;
using LMKit.TextGeneration.Chat;
using LMKitMaestro.Tests.Services;

namespace LMKitMaestro.Tests;

[Collection("LM-Kit Maestro Tests")]
public class ChatHistoryPersistenceTests
{
    [Fact]
    public async Task SubmittedPromptAndResponseArePersisted()
    {
        LMKitMaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "how much is  1 + 1 ?";
        testConversation.ConversationViewModel.Send();

        await testConversation.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptSuccessState(testConversation);

        await testConversation.DatabaseSyncTask.Task;
        var savedConversations = await testService.Database.GetConversations();
        Assert.Single(savedConversations);

        var conversationLog = savedConversations[0];
        Assert.NotNull(conversationLog.ChatHistoryData);
        var chatHistory = ChatHistory.Deserialize(conversationLog.ChatHistoryData);
        Assert.Equal(2, CountUserAndAssistantMessages(chatHistory));
    }

    [Fact]
    public async Task MessageCorrectlyPersistedWhenUnloadingModelDuringGeneration()
    {
        LMKitMaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "how much is  1 + 1 ?";
        testConversation.ConversationViewModel.Send();

        // Adding a delay before requesting model unload:
        // If we are unloading the model right after sending the request, we don't expect the conversation log to be saved in the database:
        // If the prompt request was cancelled before arriving until Lm-Kit library, the ChatHistory instance is not touched
        // -> in such scenario we simply update the UI to show the message was cancelled but its content is never restored in between sessions.
        await Task.Delay(500);
        bool unloadingSuccess = await testService.UnloadModel();
        Assert.True(unloadingSuccess);

        await testConversation.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptCancelledState(testConversation);

        var messages = testConversation.ConversationViewModel.Messages.Count;
        await testConversation.DatabaseSyncTask.Task;
        var savedConversations = await testService.Database.GetConversations();
        Assert.Single(savedConversations);

        var conversationLog = savedConversations[0];
        Assert.NotNull(conversationLog.ChatHistoryData);
        var chatHistory = ChatHistory.Deserialize(conversationLog.ChatHistoryData);
        Assert.Equal(2, CountUserAndAssistantMessages(chatHistory));
    }

    [Fact]
    public async Task ChatHistoryIsRestored()
    {
        LMKitMaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel(LMKitMaestroTestsService.Model2);
        Assert.True(loadingSuccess);

        ConversationLog dummyConversationLog = new()
        {
            ChatHistoryData = LMKitMaestroTestsHelpers.GetTestChatHistoryData(),
        };

        await testService.Database.SaveConversation(dummyConversationLog);

        await testService.ConversationListViewModel.LoadConversationLogs();
        Assert.Single(testService.ConversationListViewModel.Conversations);

        var testConversation = new ConversationViewModelWrapper(testService.ConversationListViewModel.Conversations[0]);
        testConversation.ConversationViewModel.InputText = "now add 69";
        testConversation.ConversationViewModel.Send();
        await testConversation.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptSuccessState(testConversation, 4);

        await testConversation.DatabaseSyncTask.Task;
        var savedConversations = await testService.Database.GetConversations();
        Assert.Single(savedConversations);

        var conversationLog = savedConversations[0];
        Assert.NotNull(conversationLog.ChatHistoryData);
        var chatHistory = ChatHistory.Deserialize(conversationLog.ChatHistoryData);
        Assert.Equal(4, CountUserAndAssistantMessages(chatHistory));

        var lastMessage = chatHistory.Messages.Last();
        Assert.Equal(AuthorRole.Assistant, lastMessage.AuthorRole);
        //Assert.Contains("71", lastMessage.Content);
    }


    private static int CountUserAndAssistantMessages(ChatHistory chatHistory)
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
}
