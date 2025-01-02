using LMKit.Maestro.Tests.Services;

namespace LMKit.Maestro.Tests;

[Collection("Maestro Tests")]
public class ChatPageViewModelTests
{
    [Fact]
    public void AddNewConversation()
    {
        var testService = new MaestroTestsService();
        var chatPageViewModel = testService.ChatPageViewModel;

        chatPageViewModel.StartNewConversation();

        Assert.Single(chatPageViewModel.ConversationListViewModel.Conversations);
        Assert.True(chatPageViewModel.ConversationListViewModel.CurrentConversation == chatPageViewModel.ConversationListViewModel.Conversations[0]);
    }


    [Fact]
    public async Task DeleteConversation()
    {
        var testService = new MaestroTestsService();
        var chatPageViewModel = testService.ChatPageViewModel;

        // Starting 2 conversations
        testService.ChatPageViewModel.StartNewConversation();
        testService.ChatPageViewModel.StartNewConversation();
        Assert.Equal(2, chatPageViewModel.ConversationListViewModel.Conversations.Count);
        Assert.True(chatPageViewModel.ConversationListViewModel.CurrentConversation == chatPageViewModel.ConversationListViewModel.Conversations[0]);

        var conversationToDelete = chatPageViewModel.ConversationListViewModel.Conversations[1];

        await testService.ChatPageViewModel.DeleteConversation(conversationToDelete);

        Assert.Single(chatPageViewModel.ConversationListViewModel.Conversations);
        Assert.True(chatPageViewModel.ConversationListViewModel.CurrentConversation != conversationToDelete);
    }

    [Fact]
    public async Task DeleteConversationWhileGeneratingResponse()
    {
        var testService = new MaestroTestsService();

        testService.LMKitService.LMKitConfig.RequestTimeout = 60;
        var chatPageViewModel = testService.ChatPageViewModel;

        bool loadingSuccess = await testService.LoadModel(MaestroTestsService.Model2);
        Assert.True(loadingSuccess);

        testService.ChatPageViewModel.StartNewConversation();
        Assert.Single(chatPageViewModel.ConversationListViewModel.Conversations);
        Assert.True(chatPageViewModel.ConversationListViewModel.CurrentConversation == chatPageViewModel.ConversationListViewModel.Conversations[0]);

        var conversation = new ConversationViewModelWrapper(chatPageViewModel.ConversationListViewModel.CurrentConversation);

        conversation.ConversationViewModel!.InputText = "tell me a story";
        conversation.ConversationViewModel.Submit();
        await Task.Delay(500);

        await conversation.ConversationViewModel.Cancel();

        try
        {
            await conversation.PromptResultTask.Task.WaitAsync(TimeSpan.FromSeconds(1));
        }
        catch (TimeoutException)
        {
            // Ensuring that the response generation is cancelled within a reasonable time (1s),
            // (Request time out has been previously set to 60 sec for this test)
            Assert.Fail("The response generation was not cancelled within less than 1 sec");
        }

        TestsHelpers.AssertConversationPromptCancelledState(conversation);
    }
}
