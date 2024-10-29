using LMKitMaestro.Models;
using LMKit.TextGeneration.Chat;

namespace LMKitMaestroTests;
public class ChatHistoryPersistenceTests
{
    [Fact]
    public async Task SubmittedPromptAndResponseArePersisted()
    {
        LMKitMaestroTestsHelperService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "how much is  1 + 1 ?";
        testConversation.ConversationViewModel.Send();

        await testConversation.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptSuccessState(testConversation);

        await testConversation.DatabaseSyncTask.Task;
        var savedConversations = await testService.Database.GetConversations();
        Assert.True(savedConversations.Count == 1);

        var conversationLog = savedConversations[0];
        Assert.True(conversationLog.ChatHistoryData != null);
        var chatHistory = ChatHistory.Deserialize(conversationLog.ChatHistoryData);
        Assert.True(CountUserAndAssistantMessages(chatHistory) == 2);
    }

    [Fact]
    public async Task MessageCorrectlyPersistedWhenUnloadingModelDuringGeneration()
    {
        LMKitMaestroTestsHelperService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "how much is  1 + 1 ?";
        testConversation.ConversationViewModel.Send();

        // Adding a delay before requesting model unload:
        // If we are unloading the model right after sending the requestwe don't expect the conversation log to be saved in the database:
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
        Assert.True(savedConversations.Count == 1);

        var conversationLog = savedConversations[0];
        Assert.True(conversationLog.ChatHistoryData != null);
        var chatHistory = ChatHistory.Deserialize(conversationLog.ChatHistoryData);
        Assert.True(CountUserAndAssistantMessages(chatHistory) == 2);
    }

    [Fact]
    public async Task ChatHistoryIsRestored()
    {
        LMKitMaestroTestsHelperService testService = new();
        bool loadingSuccess = await testService.LoadModel(LMKitMaestroTestsHelperService.Model2);
        Assert.True(loadingSuccess);

        ConversationLog dummyConversationLog = new ConversationLog()
        {
            ChatHistoryData = LMKitMaestroTestsHelpers.GetTestChatHistoryData(),
        };

        await testService.Database.SaveConversation(dummyConversationLog);

        await testService.ConversationListViewModel.LoadConversationLogs();
        Assert.True(testService.ConversationListViewModel.Conversations.Count == 1);

        var testConversation = new ConversationViewModelWrapper(testService.ConversationListViewModel.Conversations[0]);
        testConversation.ConversationViewModel.InputText = "now add 69";
        testConversation.ConversationViewModel.Send();
        await testConversation.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptSuccessState(testConversation, 4);

        await testConversation.DatabaseSyncTask.Task;
        var savedConversations = await testService.Database.GetConversations();
        Assert.True(savedConversations.Count == 1);

        var conversationLog = savedConversations[0];
        Assert.True(conversationLog.ChatHistoryData != null);
        var chatHistory = ChatHistory.Deserialize(conversationLog.ChatHistoryData);
        Assert.True(CountUserAndAssistantMessages(chatHistory) == 4);

        var lastMessage = chatHistory.Messages.Last();
        Assert.True(lastMessage.AuthorRole == AuthorRole.Assistant);
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
