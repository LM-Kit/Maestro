using LMKit.Maestro.Tests;
using LMKit.Maestro.Tests.Services;
using LMKit.TextGeneration.Chat;
using LMKit.Maestro.Models;

namespace Maestro.Tests;

[Collection("Maestro Tests")]
public class ChatTests
{
    [Fact]
    public async Task SubmittedPromptAndResponseArePersisted()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "how much is  1 + 1 ?";
        testConversation.ConversationViewModel.Submit();

        await testConversation.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptSuccessState(testConversation);

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
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "how much is  1 + 1 ?";
        testConversation.ConversationViewModel.Submit();

        // Adding a delay before requesting model unload:
        // If we are unloading the model right after sending the request, we don't expect the conversation log to be saved in the database:
        // If the prompt request was cancelled before arriving until Lm-Kit library, the ChatHistory instance is not touched
        // -> in such scenario we simply update the UI to show the message was cancelled but its content is never restored in between sessions.
        await Task.Delay(500);
        bool unloadingSuccess = await testService.UnloadModel();
        Assert.True(unloadingSuccess);

        await testConversation.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptCancelledState(testConversation);

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
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        ConversationLog dummyConversationLog = new()
        {
            ChatHistoryData = TestsHelpers.GetTestChatHistoryData(),
        };

        await testService.Database.SaveConversation(dummyConversationLog);

        await testService.ConversationListViewModel.LoadConversationLogs();
        Assert.Single(testService.ConversationListViewModel.Conversations);

        var testConversation = new ConversationViewModelWrapper(testService.ConversationListViewModel.Conversations[0]);
        testConversation.ConversationViewModel.InputText = "now add 69";
        testConversation.ConversationViewModel.Submit();
        await testConversation.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptSuccessState(testConversation, 4);

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

    [Fact]
    public async Task Submit1Prompt()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "hello";
        testConversation.ConversationViewModel.Submit();

        await testConversation.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptSuccessState(testConversation);
    }

    [Fact]
    public async Task CancelPromptTest()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "tell me a story";
        testConversation.ConversationViewModel.Submit();

        await Task.Delay(50);
        await testConversation.ConversationViewModel.Cancel();

        await testConversation.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptCancelledState(testConversation);
    }

    [Fact]
    public async Task Submit2PromptsCancel1()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        ConversationViewModelWrapper conversation1 = testService.GetNewConversationViewModel();
        ConversationViewModelWrapper conversation2 = testService.GetNewConversationViewModel();

        conversation1.ConversationViewModel.InputText = "tell me breaking bad";
        conversation2.ConversationViewModel.InputText = "hello";

        conversation1.ConversationViewModel.Submit();
        conversation2.ConversationViewModel.Submit();

        await Task.Delay(50);
        await conversation1.ConversationViewModel.Cancel();


        await conversation1.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptCancelledState(conversation1);

        await conversation2.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptSuccessState(conversation2);
    }


    [Fact]
    public async Task GeneratesTitle()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "1+1";
        testConversation.ConversationViewModel.Submit();

        await testConversation.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptSuccessState(testConversation);

        var title = await testConversation.TitleGenerationTask.Task;
        Assert.False(string.IsNullOrEmpty(title));
    }

    [Fact]
    public async Task Generates2Titles()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);


        var conversation1 = testService.GetNewConversationViewModel();
        conversation1.ConversationViewModel.InputText = "1+1";
        conversation1.ConversationViewModel.Submit();

        var conversation2 = testService.GetNewConversationViewModel();
        conversation2.ConversationViewModel.InputText = "what is the capital of france ?";
        conversation2.ConversationViewModel.Submit();

        await conversation1.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptSuccessState(conversation1);
        await conversation2.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptSuccessState(conversation2);

        var title1 = await conversation1.TitleGenerationTask.Task;
        Assert.False(string.IsNullOrEmpty(title1));
        var title2 = await conversation1.TitleGenerationTask.Task;
        Assert.False(string.IsNullOrEmpty(title2));
    }

    [Fact]
    public async Task UnloadDuringTitleGeneration()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var conversation = testService.GetNewConversationViewModel();
        conversation.ConversationViewModel.InputText = "1+1";
        conversation.ConversationViewModel.Submit();

        await conversation.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptSuccessState(conversation);

        // Delay the call to Unload so that it happens while the title is being generated.
        await Task.Delay(500);
        bool modelUnloaded = await testService.UnloadModel();
        Assert.True(modelUnloaded);

        var title1 = await conversation.TitleGenerationTask.Task;
        Assert.False(string.IsNullOrEmpty(title1));
    }

    [Fact]
    public async Task RegeneratesResponse()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "hello";
        testConversation.ConversationViewModel.Submit();

        await testConversation.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptSuccessState(testConversation);

        var lastMessageViewModel = testConversation.ConversationViewModel.Messages.Last();
        testConversation.ConversationViewModel.RegenerateResponseCommand.Execute(lastMessageViewModel);
        await testConversation.PromptResultTask.Task;
        TestsHelpers.AssertConversationPromptSuccessState(testConversation);
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
