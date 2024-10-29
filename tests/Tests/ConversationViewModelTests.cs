namespace LMKitMaestroTests;

public class ConversationViewModelTests
{
    [Fact]
    public async Task SendsAndReceivesResponse()
    {
        LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "hello";
        testConversation.ConversationViewModel.Send();

        await testConversation.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptSuccessState(testConversation);
    }

    [Fact]
    public async Task CancelPromptAfter1sec()
    {
        LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "tell me a fucking story";
        testConversation.ConversationViewModel.Send();

        await Task.Delay(1000);

        await testConversation.ConversationViewModel.Cancel();

        await testConversation.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptCancelledState(testConversation);
    }

    [Fact]
    public async Task CancelPromptAfter50Ms()
    {
        LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "tell me a fucking story";
        testConversation.ConversationViewModel.Send();

        await Task.Delay(50);

        await testConversation.ConversationViewModel.Cancel();

        await testConversation.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptCancelledState(testConversation);
    }

    [Fact]
    public async Task CancelPromptImmediatelyAfterSubmit()
    {
        LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "tell me a fucking story";
        testConversation.ConversationViewModel.Send();

        await testConversation.ConversationViewModel.Cancel();

        await testConversation.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptCancelledState(testConversation);
    }

    [Fact]
    public async Task SubmitTwoPromptThenCancelFirst()
    {
        LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        ConversationViewModelWrapper conversation1 = testService.GetNewConversationViewModel();
        ConversationViewModelWrapper conversation2 = testService.GetNewConversationViewModel();

        conversation1.ConversationViewModel.InputText = "tell me breaking bad";
        conversation2.ConversationViewModel.InputText = "hello";

        conversation1.ConversationViewModel.Send();
        conversation2.ConversationViewModel.Send();

        await Task.Delay(25);
        await conversation1.ConversationViewModel.Cancel();


        await conversation1.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptCancelledState(conversation1);

        await conversation2.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptSuccessState(conversation2);
    }


    [Fact]
    public async Task GenerateConversationTitle()
    {
        LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var testConversation = testService.GetNewConversationViewModel();

        testConversation.ConversationViewModel.InputText = "Comment écrire une page web de mentions legales ?";
        testConversation.ConversationViewModel.Send();

        await testConversation.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptSuccessState(testConversation);

        var title = await testConversation.TitleGenerationTask.Task;
        Assert.False(string.IsNullOrEmpty(title));
    }

    [Fact]
    public async Task GenerateConversationTitle2Conversations()
    {
        LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        
        var conversation1 = testService.GetNewConversationViewModel();
        conversation1.ConversationViewModel.InputText = "1+1";
        conversation1.ConversationViewModel.Send();

        var conversation2 = testService.GetNewConversationViewModel();
        conversation2.ConversationViewModel.InputText = "what is the capital of france ?";
        conversation2.ConversationViewModel.Send();

        await conversation1.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptSuccessState(conversation1);
        await conversation2.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptSuccessState(conversation2);

        var title1 = await conversation1.TitleGenerationTask.Task;
        Assert.False(string.IsNullOrEmpty(title1));
        var title2 = await conversation1.TitleGenerationTask.Task;
        Assert.False(string.IsNullOrEmpty(title2));
    }

    [Fact]
    public async Task UnloadModelWhileGeneratingTitle()
    {
        LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
        testService.LmKitService.LMKitConfig.RequestTimeout = 30;
        bool loadingSuccess = await testService.LoadModel();
        Assert.True(loadingSuccess);

        var conversation = testService.GetNewConversationViewModel();
        conversation.ConversationViewModel.InputText = "1+1";
        conversation.ConversationViewModel.Send();

        await conversation.PromptResultTask.Task;
        LMKitMaestroTestsHelpers.AssertConversationPromptSuccessState(conversation);

        // Delay the call to Unload so that it happens while the title is being generated.
        await Task.Delay(500);
        bool modelUnloaded = await testService.UnloadModel();
        Assert.True(modelUnloaded);

        var title1 = await conversation.TitleGenerationTask.Task;
        Assert.False(string.IsNullOrEmpty(title1));
    }
}