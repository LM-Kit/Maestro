using LMKit.Maestro.Tests.Services;

namespace LMKit.Maestro.Tests;

[Collection("Maestro Tests")]
public class ConversationViewModelTests
{
    [Fact]
    public async Task SendsAndReceivesResponse()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "hello";
        testConversation.ConversationViewModel.Submit();

        await testConversation.PromptResultTask.Task;
        MaestroTestsHelpers.AssertConversationPromptSuccessState(testConversation);
    }

    [Fact]
    public async Task CancelPromptAfter1sec()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "tell me a story";
        testConversation.ConversationViewModel.Submit();

        await Task.Delay(1000);

        await testConversation.ConversationViewModel.Cancel();

        await testConversation.PromptResultTask.Task;
        MaestroTestsHelpers.AssertConversationPromptCancelledState(testConversation);
    }

    [Fact]
    public async Task CancelPromptAfter50Ms()
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
        MaestroTestsHelpers.AssertConversationPromptCancelledState(testConversation);
    }

    [Fact]
    public async Task CancelPromptImmediatelyAfterSubmit()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "tell me a story";
        testConversation.ConversationViewModel.Submit();

        await testConversation.ConversationViewModel.Cancel();

        await testConversation.PromptResultTask.Task;
        MaestroTestsHelpers.AssertConversationPromptCancelledState(testConversation);
    }

    [Fact]
    public async Task SubmitTwoPromptThenCancelFirst()
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

        await Task.Delay(25);
        await conversation1.ConversationViewModel.Cancel();


        await conversation1.PromptResultTask.Task;
        MaestroTestsHelpers.AssertConversationPromptCancelledState(conversation1);

        await conversation2.PromptResultTask.Task;
        MaestroTestsHelpers.AssertConversationPromptSuccessState(conversation2);
    }


    [Fact]
    public async Task GenerateConversationTitle()
    {
        MaestroTestsService testService = new();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "Comment écrire une page web de mentions legales ?";
        testConversation.ConversationViewModel.Submit();

        await testConversation.PromptResultTask.Task;
        MaestroTestsHelpers.AssertConversationPromptSuccessState(testConversation);

        var title = await testConversation.TitleGenerationTask.Task;
        Assert.False(string.IsNullOrEmpty(title));
    }

    [Fact]
    public async Task GenerateConversationTitle2Conversations()
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
        MaestroTestsHelpers.AssertConversationPromptSuccessState(conversation1);
        await conversation2.PromptResultTask.Task;
        MaestroTestsHelpers.AssertConversationPromptSuccessState(conversation2);

        var title1 = await conversation1.TitleGenerationTask.Task;
        Assert.False(string.IsNullOrEmpty(title1));
        var title2 = await conversation1.TitleGenerationTask.Task;
        Assert.False(string.IsNullOrEmpty(title2));
    }

    [Fact]
    public async Task UnloadModelWhileGeneratingTitle()
    {
        MaestroTestsService testService = new();
        testService.LMKitService.LMKitConfig.RequestTimeout = 30;
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var conversation = testService.GetNewConversationViewModel();
        conversation.ConversationViewModel.InputText = "1+1";
        conversation.ConversationViewModel.Submit();

        await conversation.PromptResultTask.Task;
        MaestroTestsHelpers.AssertConversationPromptSuccessState(conversation);

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
        MaestroTestsHelpers.AssertConversationPromptSuccessState(testConversation);

        var lastMessageViewModel = testConversation.ConversationViewModel.Messages.Last();
        testConversation.ConversationViewModel.RegenerateResponseCommand.Execute(lastMessageViewModel);
        await testConversation.PromptResultTask.Task;
        MaestroTestsHelpers.AssertConversationPromptSuccessState(testConversation);
    }
}