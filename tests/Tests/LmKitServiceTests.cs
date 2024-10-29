using LMKitMaestro.Services;
using LMKitMaestroTests.Services;

namespace LMKitMaestroTests
{
    public class LmKitServiceTests
    {
        public LmKitServiceTests()
        {
        }

        [Fact]
        public async Task LoadingModel()
        {
            LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
            bool loadingSuccess = await testService.LoadModel(LMKitMaestroTestsHelperService.Model2);
            Assert.True(loadingSuccess);
        }

        [Fact]
        public async Task LoadUnloadLoadAnother()
        {
            LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
            bool loadingSuccess = await testService.LoadModel(LMKitMaestroTestsHelperService.Model1);
            Assert.True(loadingSuccess);

            bool unloadingSuccess = await testService.UnloadModel();
            Assert.True(unloadingSuccess);

            loadingSuccess = await testService.LoadModel(LMKitMaestroTestsHelperService.Model2);
            Assert.True(loadingSuccess);
        }

        [Fact]
        public async Task LoadThenLoadAnother()
        {
            LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
            bool loadingSuccess = await testService.LoadModel(LMKitMaestroTestsHelperService.Model1);
            Assert.True(loadingSuccess);

            loadingSuccess = await testService.LoadModel(LMKitMaestroTestsHelperService.Model2);
            Assert.True(loadingSuccess);
        }


        [Fact]
        private async Task SubmitOnePrompt()
        {
            LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
            bool loadingSuccess = await testService.LoadModel();
            Assert.True(loadingSuccess);

            var conversation = testService.GetNewLmKitConversation();

            var response = await testService.LmKitService.SubmitPrompt(conversation, "1+1");
            LMKitMaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);
        }

        [Fact]
        public async Task ChangePrompt()
        {
            LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();

            testService.LmKitService.LMKitConfig.SystemPrompt = "You are a very angry chatbot and starts all your replies with 'ARGH'";
            bool loadingSuccess = await testService.LoadModel();
            Assert.True(loadingSuccess);

            var conversation = testService.GetNewLmKitConversation();
            var response = await testService.LmKitService.SubmitPrompt(conversation, "1+1");

            LMKitMaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);

            var firstMessage = conversation.ChatHistory!.Messages[0];
            Assert.True(firstMessage.AuthorRole == LMKit.TextGeneration.Chat.AuthorRole.System && firstMessage.Content == testService.LmKitService.LMKitConfig.SystemPrompt);
        }

        [Fact]
        public async Task HonorsTimeout()
        {
            LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();

            testService.LmKitService.LMKitConfig.RequestTimeout = 1;
            bool loadingSuccess = await testService.LoadModel();
            Assert.True(loadingSuccess);

            var conversation = testService.GetNewLmKitConversation();
            var response = await testService.LmKitService.SubmitPrompt(conversation, "tell me a story");

            Assert.True(response != null);
            Assert.True(response.Status == LmKitTextGenerationStatus.Cancelled);
            Assert.True(response.Exception is OperationCanceledException operationCancelled);
        }

        [Fact]
        private async Task Submit2PromptsFromDistinctConversations()
        {
            LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
            bool loadingSuccess = await testService.LoadModel();
            Assert.True(loadingSuccess);

            var conversation1 = testService.GetNewLmKitConversation();
            var conversation2 = testService.GetNewLmKitConversation();

            var response1 = await testService.LmKitService.SubmitPrompt(conversation1, "1+1");
            LMKitMaestroTestsHelpers.AssertPromptResponseIsSuccessful(response1);

            var response2 = await testService.LmKitService.SubmitPrompt(conversation2, "2+2");
            LMKitMaestroTestsHelpers.AssertPromptResponseIsSuccessful(response2);
        }

        [Fact]
        private async Task Submit3PromptsFromDistinctConversations()
        {
            LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
            bool loadingSuccess = await testService.LoadModel();
            Assert.True(loadingSuccess);

            var conversation1 = testService.GetNewLmKitConversation();
            var conversation2 = testService.GetNewLmKitConversation();

            var response = await testService.LmKitService.SubmitPrompt(conversation1, "1+1");
            LMKitMaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);

            response = await testService.LmKitService.SubmitPrompt(conversation2, "2+2");
            LMKitMaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);

            response = await testService.LmKitService.SubmitPrompt(conversation1, "3+3");
            LMKitMaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);
        }

        [Fact]
        private async Task SubmitOnePromptChangeModelSubmitAnother()
        {
            LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
            bool loadingSuccess = await testService.LoadModel(LMKitMaestroTestsHelperService.Model1);
            Assert.True(loadingSuccess);

            var conversation = testService.GetNewLmKitConversation();

            var response = await testService.LmKitService.SubmitPrompt(conversation, "2+2");
            LMKitMaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);

            loadingSuccess = await testService.LoadModel(LMKitMaestroTestsHelperService.Model2);
            Assert.True(loadingSuccess);

            response = await testService.LmKitService.SubmitPrompt(conversation, "1+1");
            LMKitMaestroTestsHelpers.AssertPromptResponseIsSuccessful(response);
        }

        // This test does not wait for the first submitted prompt's result to be obtained.
        // The first prompt operation should be cancelled right after the model change request is received.
        [Fact]
        private async Task SubmitOnePromptChangeModelSubmitAnother2()
        {
            LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
            testService.LmKitService.LMKitConfig.RequestTimeout = 30;
            bool loadingSuccess = await testService.LoadModel(LMKitMaestroTestsHelperService.Model1);
            Assert.True(loadingSuccess);

            var conversation = testService.GetNewLmKitConversation();

            var firstResponseTask = testService.LmKitService.SubmitPrompt(conversation, "tell me a story");

            var secondModelLoadingTask = testService.LoadModel(LMKitMaestroTestsHelperService.Model2);

            var firstPromptResult = await firstResponseTask;

            Assert.True(firstPromptResult.Status == LmKitTextGenerationStatus.Cancelled || firstPromptResult.Status == LmKitTextGenerationStatus.UnknownError);

            loadingSuccess = await secondModelLoadingTask;
            Assert.True(loadingSuccess);

            var secondResponse = await testService.LmKitService.SubmitPrompt(conversation, "1+1");
            LMKitMaestroTestsHelpers.AssertPromptResponseIsSuccessful(secondResponse);
        }

        [Fact]
        private async Task Submit2PromptsFromDistinctConversationsThenUnloadModel()
        {
            LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
            bool loadingSuccess = await testService.LoadModel();
            Assert.True(loadingSuccess);

            LmKitDummyConversation conversation1 = new LmKitDummyConversation(testService.LmKitService);
            LmKitDummyConversation conversation2 = new LmKitDummyConversation(testService.LmKitService);

            conversation1.SubmitPrompt(testService.LmKitService, "bonjour");
            conversation2.SubmitPrompt(testService.LmKitService, "chaleureuses salutations");

            bool unloadingSuccess = await testService.UnloadModel();
            Assert.True(unloadingSuccess);

            var result = await conversation1.PromptResultTask.Task;
            Assert.True(result != null && result.Status == LmKitTextGenerationStatus.Cancelled);

            result = await conversation2.PromptResultTask.Task;
            Assert.True(result != null && result.Status == LmKitTextGenerationStatus.Cancelled);
        }

        [Fact]
        private async Task SubmitOnePromptThenUnloadModel()
        {
            LMKitMaestroTestsHelperService testService = new LMKitMaestroTestsHelperService();
            bool loadingSuccess = await testService.LoadModel();
            Assert.True(loadingSuccess);

            LmKitDummyConversation conversation1 = new LmKitDummyConversation(testService.LmKitService);

            conversation1.SubmitPrompt(testService.LmKitService, "bonjour");

            bool unloadingSuccess = await testService.UnloadModel();
            Assert.True(unloadingSuccess);

            var result = await conversation1.PromptResultTask.Task;
            Assert.True(result != null && result.Status == LmKitTextGenerationStatus.Cancelled);
        }
    }
}