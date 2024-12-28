using LMKit.Maestro.Models;
using LMKit.Maestro.Services;
using LMKit.Maestro.Tests.Services;

namespace LMKit.Maestro.Tests;

[Collection("Maestro Tests")]
public class LMKitServiceTests
{
    public LMKitServiceTests()
    {
    }

    [Fact]
    public async Task LoadingModel()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel(MaestroTestsService.Model1);
        Assert.True(loadingSuccess);
    }

    [Fact]
    public async Task LoadUnloadLoadAnother()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel(MaestroTestsService.Model1);
        Assert.True(loadingSuccess);

        bool unloadingSuccess = await testService.UnloadModel();
        Assert.True(unloadingSuccess);

        loadingSuccess = await testService.LoadModel(MaestroTestsService.Model2);
        Assert.True(loadingSuccess);
    }

    [Fact]
    public async Task LoadThenLoadAnother()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel(MaestroTestsService.Model1);
        Assert.True(loadingSuccess);

        loadingSuccess = await testService.LoadModel(MaestroTestsService.Model2);
        Assert.True(loadingSuccess);
    }


    [Fact]
    private async Task SubmitOnePrompt()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var conversation = testService.GetNewLMKitConversation();

        var response = await testService.LMKitService.SubmitPrompt(conversation, "1+1");
        MaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);
    }

    [Fact]
    public async Task ChangePrompt()
    {
        MaestroTestsService testService = new();

        testService.LMKitService.LMKitConfig.SystemPrompt = "You are a very angry chatbot and starts all your replies with 'ARGH'";
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var conversation = testService.GetNewLMKitConversation();
        var response = await testService.LMKitService.SubmitPrompt(conversation, "1+1");

        MaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);

        var firstMessage = conversation.ChatHistory!.Messages[0];
        Assert.True(firstMessage.AuthorRole == LMKit.TextGeneration.Chat.AuthorRole.System && firstMessage.Content == testService.LMKitService.LMKitConfig.SystemPrompt);
    }

    [Fact]
    public async Task HonorsTimeout()
    {
        MaestroTestsService testService = new();

        testService.LMKitService.LMKitConfig.RequestTimeout = 1;
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var conversation = testService.GetNewLMKitConversation();
        var response = await testService.LMKitService.SubmitPrompt(conversation, "tell me a story");

        Assert.NotNull(response);
        Assert.Equal(LMKitRequestStatus.Cancelled, response.Status);
        Assert.True(response.Exception is OperationCanceledException operationCancelled);
    }

    [Fact]
    private async Task Submit2PromptsFromDistinctConversations()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var conversation1 = testService.GetNewLMKitConversation();
        var conversation2 = testService.GetNewLMKitConversation();

        var response1 = await testService.LMKitService.SubmitPrompt(conversation1, "1+1");
        MaestroTestsHelpers.AssertPromptResponseIsSuccessful(response1);

        var response2 = await testService.LMKitService.SubmitPrompt(conversation2, "2+2");
        MaestroTestsHelpers.AssertPromptResponseIsSuccessful(response2);
    }

    [Fact]
    private async Task Submit3PromptsFromDistinctConversations()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var conversation1 = testService.GetNewLMKitConversation();
        var conversation2 = testService.GetNewLMKitConversation();

        var response = await testService.LMKitService.SubmitPrompt(conversation1, "1+1");
        MaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);

        response = await testService.LMKitService.SubmitPrompt(conversation2, "2+2");
        MaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);

        response = await testService.LMKitService.SubmitPrompt(conversation1, "3+3");
        MaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);
    }

    [Fact]
    private async Task SubmitOnePromptChangeModelSubmitAnother()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel(MaestroTestsService.Model1);
        Assert.True(loadingSuccess);

        var conversation = testService.GetNewLMKitConversation();

        var response = await testService.LMKitService.SubmitPrompt(conversation, "2+2");
        MaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);

        loadingSuccess = await testService.LoadModel(MaestroTestsService.Model2);
        Assert.True(loadingSuccess);

        response = await testService.LMKitService.SubmitPrompt(conversation, "1+1");
        MaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);
    }

    // This test does not wait for the first submitted prompt's result to be obtained.
    // The first prompt operation should be cancelled right after the model change request is received.
    [Fact]
    private async Task SubmitOnePromptChangeModelSubmitAnother2()
    {
        MaestroTestsService testService = new();
        testService.LMKitService.LMKitConfig.RequestTimeout = 30;
        bool loadingSuccess = await testService.LoadModel(MaestroTestsService.Model1);
        Assert.True(loadingSuccess);

        var conversation = testService.GetNewLMKitConversation();

        var firstResponseTask = testService.LMKitService.SubmitPrompt(conversation, "tell me a story");

        var secondModelLoadingTask = testService.LoadModel(MaestroTestsService.Model2);

        var firstPromptResult = await firstResponseTask;

        Assert.True(firstPromptResult.Status == LMKitRequestStatus.Cancelled || firstPromptResult.Status == LMKitRequestStatus.GenericError);

        loadingSuccess = await secondModelLoadingTask;
        Assert.True(loadingSuccess);

        var secondResponse = await testService.LMKitService.SubmitPrompt(conversation, "1+1");
        MaestroTestsHelpers.AssertPromptResponseIsSuccessful(secondResponse);
    }

    [Fact]
    private async Task Submit2PromptsFromDistinctConversationsThenUnloadModel()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        LMKitDummyConversation conversation1 = new(testService.LMKitService);
        LMKitDummyConversation conversation2 = new(testService.LMKitService);

        conversation1.SubmitPrompt(testService.LMKitService, "bonjour");
        conversation2.SubmitPrompt(testService.LMKitService, "chaleureuses salutations");

        bool unloadingSuccess = await testService.UnloadModel();
        Assert.True(unloadingSuccess);

        var result = await conversation1.PromptResultTask.Task;
        Assert.True(result != null && result.Status == LMKitRequestStatus.Cancelled);

        result = await conversation2.PromptResultTask.Task;
        Assert.True(result != null && result.Status == LMKitRequestStatus.Cancelled);
    }

    [Fact]
    private async Task SubmitOnePromptThenUnloadModel()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        LMKitDummyConversation conversation1 = new(testService.LMKitService);

        conversation1.SubmitPrompt(testService.LMKitService, "bonjour");

        bool unloadingSuccess = await testService.UnloadModel();
        Assert.True(unloadingSuccess);

        var result = await conversation1.PromptResultTask.Task;
        Assert.True(result != null && result.Status == LMKitRequestStatus.Cancelled);
    }

    [Fact]
    public async Task Translate()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var result = await testService.LMKitService.SubmitTranslation("est-ce que ça marche cette merde ?", LMKit.TextGeneration.Language.French);

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public async Task RegenerateResponseRestoredChatHistory()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        ConversationLog dummyConversationLog = new()
        {
            ChatHistoryData = MaestroTestsHelpers.GetTestChatHistoryData(),
        };

        await testService.Database.SaveConversation(dummyConversationLog);

        await testService.ConversationListViewModel.LoadConversationLogs();
        Assert.Single(testService.ConversationListViewModel.Conversations);


        var conversation = testService.ConversationListViewModel.Conversations.First()!;
        var lastMessage = conversation.Messages.Last().LMKitMessage;

        var response = await testService.LMKitService.RegenerateResponse(conversation.LMKitConversation, lastMessage!);

        MaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);

    }
}