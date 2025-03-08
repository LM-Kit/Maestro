using LMKit.Maestro.Models;
using LMKit.Maestro.Services;
using Maestro.Tests.Services;

namespace Maestro.Tests;

[Collection("Maestro Tests")]
public class LMKitServiceTests
{
    [Fact]
    public async Task DownloadModel()
    {
        MaestroTestsService testService = new();
        var startsModelDownload = await testService.StartModelDownload(Constants.ModelCard);

        Assert.True(startsModelDownload);
    }

    [Fact]
    public async Task LoadModel()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel(Constants.Model1);
        Assert.True(loadingSuccess);
    }

    [Fact]
    public async Task LoadUnload()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel(Constants.Model1);
        Assert.True(loadingSuccess);

        bool unloadingSuccess = await testService.UnloadModel();
        Assert.True(unloadingSuccess);

        loadingSuccess = await testService.LoadModel(Constants.Model2);
        Assert.True(loadingSuccess);
    }

    [Fact]
    public async Task Load2Models()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel(Constants.Model1);
        Assert.True(loadingSuccess);

        loadingSuccess = await testService.LoadModel(Constants.Model2);
        Assert.True(loadingSuccess);
    }


    [Fact]
    private async Task UnloadDuringResponse()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        LMKitDummyConversation conversation1 = new(testService.LMKitService);

        await conversation1.SubmitPrompt(testService.LMKitService, "bonjour");

        bool unloadingSuccess = await testService.UnloadModel();
        Assert.True(unloadingSuccess);

        var result = await conversation1.PromptResultTask.Task;
        Assert.True(result != null && result.Status == LMKitRequestStatus.Cancelled);
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
    public async Task ChangePrompt()
    {
        MaestroTestsService testService = new();

        testService.LMKitService.LMKitConfig.SystemPrompt = "you are a nice bot";
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var conversation = testService.GetNewLMKitConversation();
        var response = await testService.LMKitService.Chat.SubmitPrompt(conversation, "1+1");

        TestsHelpers.AssertPromptResponseIsSuccessful(response);

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
        var response = await testService.LMKitService.Chat.SubmitPrompt(conversation, "tell me a story");

        Assert.NotNull(response);
        Assert.Equal(LMKitRequestStatus.Cancelled, response.Status);
        Assert.True(response.Exception is OperationCanceledException operationCancelled);
    }

    [Fact]
    private async Task Submit1ChangeModelSubmit1()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel(Constants.Model1);
        Assert.True(loadingSuccess);

        var conversation = testService.GetNewLMKitConversation();

        var response = await testService.LMKitService.Chat.SubmitPrompt(conversation, "2+2");
        TestsHelpers.AssertPromptResponseIsSuccessful(response);

        loadingSuccess = await testService.LoadModel(Constants.Model2);
        Assert.True(loadingSuccess);

        response = await testService.LMKitService.Chat.SubmitPrompt(conversation, "1+1");
        TestsHelpers.AssertPromptResponseIsSuccessful(response);
    }

    // This test does not wait for the first submitted prompt's result to be obtained.
    // The first prompt operation should be cancelled right after the model change request is received.
    [Fact]
    private async Task Submit1ChangeModelSubmit1_NoAwaiting()
    {
        MaestroTestsService testService = new();
        testService.LMKitService.LMKitConfig.RequestTimeout = 30;
        bool loadingSuccess = await testService.LoadModel(Constants.Model1);
        Assert.True(loadingSuccess);

        var conversation = testService.GetNewLMKitConversation();

        var firstResponseTask = testService.LMKitService.Chat.SubmitPrompt(conversation, "tell me a story");

        var secondModelLoadingTask = testService.LoadModel(Constants.Model2);

        var firstPromptResult = await firstResponseTask;

        Assert.True(firstPromptResult.Status == LMKitRequestStatus.Cancelled || firstPromptResult.Status == LMKitRequestStatus.GenericError);

        loadingSuccess = await secondModelLoadingTask;
        Assert.True(loadingSuccess);

        var secondResponse = await testService.LMKitService.Chat.SubmitPrompt(conversation, "1+1");
        TestsHelpers.AssertPromptResponseIsSuccessful(secondResponse);
    }

    [Fact]
    private async Task Submit2From2ChatsUnload()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        LMKitDummyConversation conversation1 = new(testService.LMKitService);
        LMKitDummyConversation conversation2 = new(testService.LMKitService);

        await conversation1.SubmitPrompt(testService.LMKitService, "bonjour");
        await conversation2.SubmitPrompt(testService.LMKitService, "chaleureuses salutations");

        bool unloadingSuccess = await testService.UnloadModel();
        Assert.True(unloadingSuccess);

        var result = await conversation1.PromptResultTask.Task;
        Assert.True(result != null && result.Status == LMKitRequestStatus.Cancelled);

        result = await conversation2.PromptResultTask.Task;
        Assert.True(result != null && result.Status == LMKitRequestStatus.Cancelled);
    }

    [Fact]
    private async Task RegenerateSubmittedPromptResponse()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var conversation = testService.GetNewLMKitConversation();

        var response = await testService.LMKitService.Chat.SubmitPrompt(conversation, "1+1");
        TestsHelpers.AssertPromptResponseIsSuccessful(response);

        var lastMessage = conversation.ChatHistory!.Messages.Last();
        response = await testService.LMKitService.Chat.RegenerateResponse(conversation, lastMessage!);

        TestsHelpers.AssertPromptResponseIsSuccessful(response);
    }

    [Fact]
    public async Task RegenerateRestoredMessageResponse()
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


        var conversation = testService.ConversationListViewModel.Conversations.First()!;
        var lastMessage = conversation.Messages.Last().LMKitMessage;

        var response = await testService.LMKitService.Chat.RegenerateResponse(conversation.LMKitConversation, lastMessage!);

        TestsHelpers.AssertPromptResponseIsSuccessful(response);
    }
}