using LMKitMaestro.Tests.Services;

namespace LMKitMaestro.Tests;

public class ChatPageViewModelTests
{
    [Fact]
    public void AddNewConversation()
    {
        var testService = new LMKitMaestroTestsService();
        var chatPageViewModel = testService.ChatPageViewModel;

        chatPageViewModel.StartNewConversation();

        Assert.Equal(1, chatPageViewModel.ConversationListViewModel.Conversations.Count);
        Assert.True(chatPageViewModel.ConversationListViewModel.CurrentConversation == chatPageViewModel.ConversationListViewModel.Conversations[0]);
    }


    [Fact]
    public async Task DeleteConversation()
    {
        var testService = new LMKitMaestroTestsService();
        var chatPageViewModel = testService.ChatPageViewModel;

        // Starting 2 conversations
        testService.ChatPageViewModel.StartNewConversation();
        testService.ChatPageViewModel.StartNewConversation();
        Assert.Equal(2, chatPageViewModel.ConversationListViewModel.Conversations.Count);
        Assert.True(chatPageViewModel.ConversationListViewModel.CurrentConversation == chatPageViewModel.ConversationListViewModel.Conversations[0]);

        var conversationToDelete = chatPageViewModel.ConversationListViewModel.Conversations[1];

        await testService.ChatPageViewModel.DeleteConversation(conversationToDelete);

        Assert.Equal(1, chatPageViewModel.ConversationListViewModel.Conversations.Count);
        Assert.True(chatPageViewModel.ConversationListViewModel.CurrentConversation != conversationToDelete);
    }

    [Fact]
    public async Task DeleteConversationWhileGeneratingResponse()
    {
        var testService = new LMKitMaestroTestsService();

        testService.LmKitService.LMKitConfig.RequestTimeout = 60;
        var chatPageViewModel = testService.ChatPageViewModel;

        bool loadingSuccess = await testService.LoadModel(LMKitMaestroTestsService.Model2);
        Assert.True(loadingSuccess);

        testService.ChatPageViewModel.StartNewConversation();
        Assert.Equal(1, chatPageViewModel.ConversationListViewModel.Conversations.Count);
        Assert.True(chatPageViewModel.ConversationListViewModel.CurrentConversation == chatPageViewModel.ConversationListViewModel.Conversations[0]);

        var conversation = new ConversationViewModelWrapper(chatPageViewModel.ConversationListViewModel.CurrentConversation);

        conversation.ConversationViewModel!.InputText = "tell me a story";
        conversation.ConversationViewModel.Send();
        await Task.Delay(500);

        await conversation.ConversationViewModel.Cancel();
        // Ensuring that the response generation is cancelled within a reasonable time (1s),
        // (Request time out has been previously set to 60 sec for this test)
        bool isCancelledWithin1second = await conversation.PromptResultTask.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.True(isCancelledWithin1second);
        LMKitMaestroTestsHelpers.AssertConversationPromptCancelledState(conversation);
    }
}
