using LMKitMaestro.Data;
using LMKitMaestro.Services;
using LMKitMaestro.ViewModels;
using LMKitMaestroTests.Services;

namespace LMKitMaestroTests
{
    internal class LMKitMaestroTestsHelperService
    {
        public static readonly ModelInfo Model1 = new ModelInfo("lmKit", "llama-3-8b-instruct-gguf", "Llama-3-8B-Q4_K_M.gguf", new Uri(@"D:\_models\lm-kit\llama-3-8b-instruct-gguf\Llama-3-8B-Q4_K_M.gguf"));
        public static readonly ModelInfo Model2 = new ModelInfo("lm-kit", "phi-3.1-mini-4k-3.8b-instruct-gguf", "Phi-3.5-mini-Instruct-Q4_K_M.gguf", new Uri(@"D:\_models\lm-kit\phi-3.1-mini-4k-3.8b-instruct-gguf\Phi-3.5-mini-Instruct-Q4_K_M.gguf.gguf"));

        private Exception? _errorLoadingException;
        TaskCompletionSource<bool>? _modelLoadingTask;
        TaskCompletionSource<bool>? _modelUnloadedTask;

        public bool ProgressEventWasRaided { get; private set; }

        public ILLMFileManager LLmFileManager { get; } = new DummyLLmFileManager();
        public LMKitService LmKitService { get; } = new LMKitService();
        public ILMKitMaestroDatabase Database { get; } = new DummyLMKitMaestroDatabase();
        public ConversationListViewModel ConversationListViewModel { get; }

        public ChatPageViewModel ChatPageViewModel { get; }

        public LMKitMaestroTestsHelperService()
        {
            ConversationListViewModel = GetNewConversationListViewModel(LmKitService, Database);
            ChatPageViewModel = GetNewChatPageViewModel(LmKitService, ConversationListViewModel, Database, LLmFileManager);
            LmKitService.LMKitConfig.MaximumCompletionTokens = 200;
            LmKitService.LMKitConfig.RequestTimeout = 15;
        }

        public LMKitService.Conversation GetNewLmKitConversation()
        {
            return new LMKitService.Conversation(LmKitService);
        }

        public ConversationViewModelWrapper GetNewConversationViewModel()
        {
            ConversationListViewModel.AddNewConversation();

            return new ConversationViewModelWrapper(ConversationListViewModel.CurrentConversation!);
        }

        public async Task<bool> LoadModel(ModelInfo? modelInfo = null)
        {
            if (modelInfo == null)
            {
                modelInfo = Model1;
            }

            _modelLoadingTask = new TaskCompletionSource<bool>();
            ProgressEventWasRaided = false;
            LmKitService.ModelLoadingProgressed += LmKitService_ModelLoadingProgressed;
            LmKitService.ModelLoadingCompleted += LmKitService_ModelLoadingCompleted;
            LmKitService.ModelLoadingFailed += LmKitService_ModelLoadingFailed;

            LmKitService.LoadModel(modelInfo.FileUri!);


            var result = await _modelLoadingTask.Task;

            return result;
        }

        public async Task<bool> UnloadModel()
        {
            _modelUnloadedTask = new TaskCompletionSource<bool>();
            LmKitService.ModelUnloaded += OnModelUnloaded;
            LmKitService.UnloadModel();


            var result = await _modelUnloadedTask.Task;

            return result;
        }

        private void OnModelUnloaded(object? sender, EventArgs e)
        {
            _modelUnloadedTask?.SetResult(true);
        }

        private void LmKitService_ModelLoadingProgressed(object? sender, EventArgs e)
        {
            var args = (LMKitService.ModelLoadingProgressedEventArgs)e;
            ProgressEventWasRaided = true;
        }

        private void LmKitService_ModelLoadingCompleted(object? sender, EventArgs e)
        {
            _modelLoadingTask?.TrySetResult(true);
        }

        private void LmKitService_ModelLoadingFailed(object? sender, EventArgs e)
        {
            var args = (LMKitService.ModelLoadingFailedEventArgs)e;

            _errorLoadingException = args.Exception;
            _modelLoadingTask?.TrySetResult(false);
        }

        private static ConversationViewModel GetNewConversationViewModel(LMKitService lmKitService, ILMKitMaestroDatabase database)
        {
            throw new NotImplementedException();

            //Mock<IPopupService> popupService = new Mock<IPopupService>();
            //IAppSettingsService appSettingsService = new AppSettingsService(new Mock<IPreferences>().Object);
            //return new ConversationViewModel(appSettingsService, lmKitService, database, popupService.Object);
        }

        private static ConversationListViewModel GetNewConversationListViewModel(LMKitService lmKitService, ILMKitMaestroDatabase database)
        {
            throw new NotImplementedException();
            //Mock<ILogger<ConversationListViewModel>> logger = new Mock<ILogger<ConversationListViewModel>>();
            //Mock<IPopupService> popupService = new Mock<IPopupService>();
            //IAppSettingsService appSettingsService = new AppSettingsService(new Mock<IPreferences>().Object);

            //return new ConversationListViewModel(logger.Object, popupService.Object, database, lmKitService, appSettingsService);
        }

        private static ChatPageViewModel GetNewChatPageViewModel(LMKitService lmKitService, ConversationListViewModel conversationListViewModel, ILMKitMaestroDatabase database,  ILLMFileManager llmFileManager)
        {
            throw new NotImplementedException();

            //ModelListViewModel modelListViewModel = new ModelListViewModel(llmFileManager, lmKitService);
            //Mock<ILogger<ChatPageViewModel>> logger = new Mock<ILogger<ChatPageViewModel>>();
            //Mock<IPopupService> popupService = new Mock<IPopupService>();
            //AppSettingsService appSettingsService = new AppSettingsService(new Mock<IPreferences>().Object);
            //SettingsViewModel settingsViewModel = new SettingsViewModel(appSettingsService, lmKitService);

            //return new ChatPageViewModel(conversationListViewModel, modelListViewModel, logger.Object, popupService.Object, database, lmKitService, llmFileManager, settingsViewModel); 
        }
    }
}
