using LMKit.Maestro.Data;
using LMKit.Maestro.Helpers;
using LMKit.Maestro.Services;
using LMKit.Maestro.Tests.Services;
using LMKit.Maestro.ViewModels;
using Microsoft.Extensions.Logging;
using Mopups.Interfaces;
using Moq;

namespace LMKit.Maestro.Tests
{
    internal class MaestroTestsService
    {
        public static readonly Uri Model1 = new(@"https://huggingface.co/lm-kit/llama-3.2-1b-instruct.gguf/resolve/main/Llama-3.2-1B-Instruct-Q4_K_M.gguf?download=true");
        public static readonly Uri Model2 = new(@"https://huggingface.co/lm-kit/qwen-2.5-0.5b-instruct-gguf/resolve/main/Qwen-2.5-0.5B-Instruct-Q4_K_M.gguf?download=true");

        private Exception? _errorLoadingException;
        TaskCompletionSource<bool>? _modelLoadingTask;
        TaskCompletionSource<bool>? _modelUnloadedTask;

        public bool ProgressEventWasRaided { get; private set; }

        public ILLMFileManager LLmFileManager { get; } = new DummyLLmFileManager();
        public LMKitService LMKitService { get; } = new LMKitService();
        public IMaestroDatabase Database { get; } = new DummyMaestroDatabase();

        public SettingsViewModel SettingsViewModel { get; }
        public ConversationListViewModel ConversationListViewModel { get; }
        public ModelListViewModel ModelListViewModel { get; }
        public ChatPageViewModel ChatPageViewModel { get; }

        public MaestroTestsService()
        {
            SettingsViewModel = GetNewSettingsViewModel(LMKitService);
            ConversationListViewModel = GetNewConversationListViewModel(LMKitService, Database);
            ModelListViewModel = GetNewModelListViewModel(LMKitService, LLmFileManager);
            ChatPageViewModel = GetNewChatPageViewModel(LMKitService, ConversationListViewModel, ModelListViewModel, Database, LLmFileManager, SettingsViewModel);
            LMKitService.LMKitConfig.MaximumCompletionTokens = 200;
            LMKitService.LMKitConfig.RequestTimeout = 15;
        }

        public LMKitService.Conversation GetNewLMKitConversation()
        {
            return new LMKitService.Conversation(LMKitService);
        }

        public ConversationViewModelWrapper GetNewConversationViewModel()
        {
            ConversationListViewModel.AddNewConversation();

            return new ConversationViewModelWrapper(ConversationListViewModel.CurrentConversation!);
        }

        public async Task<bool> LoadModel(Uri? modelUri = null)
        {
            if (modelUri == null)
            {
                modelUri = Model1;
            }

            _modelLoadingTask = new TaskCompletionSource<bool>();
            ProgressEventWasRaided = false;
            LMKitService.ModelLoadingProgressed += LMKitService_ModelLoadingProgressed;
            LMKitService.ModelLoadingCompleted += LMKitService_ModelLoadingCompleted;
            LMKitService.ModelLoadingFailed += LMKitService_ModelLoadingFailed;

            string? localFilePath = FileHelpers.GetModelFilePathFromUrl(modelUri, LMKitDefaultSettings.DefaultModelsFolderPath);

            if (localFilePath == null)
            {
                throw new Exception("Failed to create local file path from model download url");
            }

            LMKitService.LoadModel(modelUri!, localFilePath);


            var result = await _modelLoadingTask.Task;

            return result;
        }

        public async Task<bool> UnloadModel()
        {
            _modelUnloadedTask = new TaskCompletionSource<bool>();
            LMKitService.ModelUnloaded += OnModelUnloaded;
            LMKitService.UnloadModel();


            var result = await _modelUnloadedTask.Task;

            return result;
        }

        private void OnModelUnloaded(object? sender, EventArgs e)
        {
            _modelUnloadedTask?.SetResult(true);
        }

        private void LMKitService_ModelLoadingProgressed(object? sender, EventArgs e)
        {
            var args = (LMKitService.ModelLoadingProgressedEventArgs)e;
            ProgressEventWasRaided = true;
        }

        private void LMKitService_ModelLoadingCompleted(object? sender, EventArgs e)
        {
            _modelLoadingTask?.TrySetResult(true);
        }

        private void LMKitService_ModelLoadingFailed(object? sender, EventArgs e)
        {
            var args = (LMKitService.ModelLoadingFailedEventArgs)e;

            _errorLoadingException = args.Exception;
            _modelLoadingTask?.TrySetResult(false);
        }

        private static ConversationViewModel GetNewConversationViewModel(LMKitService lmKitService, IMaestroDatabase database)
        {
            var popupService = new Mock<IPopupService>().Object;
            var mainThread = new Mock<IMainThread>().Object;
            var appSettingsService = new Mock<IAppSettingsService>().Object;

            return new ConversationViewModel(mainThread, popupService, appSettingsService, lmKitService, database);
        }

        private static SettingsViewModel GetNewSettingsViewModel(LMKitService lmKitService)
        {
            var appSettingsService = new Mock<IAppSettingsService>().Object;

            return new SettingsViewModel(appSettingsService, lmKitService);
        }

        private static ConversationListViewModel GetNewConversationListViewModel(LMKitService lmKitService, IMaestroDatabase database)
        {
            var popupService = new Mock<IPopupService>().Object;
            var mainThread = new Mock<IMainThread>().Object;
            var appSettingsService = new Mock<IAppSettingsService>().Object;
            var logger = new Mock<ILogger<ConversationListViewModel>>().Object;

            return new ConversationListViewModel(mainThread, popupService, logger, database, lmKitService, appSettingsService);
        }

        private static ModelListViewModel GetNewModelListViewModel(LMKitService lmKitService, ILLMFileManager llmFileManager)
        {
            var mainThread = new Mock<IMainThread>().Object;
            var navigationService = new Mock<INavigationService>().Object;
            var popupNavigation = new Mock<IPopupNavigation>().Object;
            var popupService = new Mock<IPopupService>().Object;

            return new ModelListViewModel(mainThread, llmFileManager, lmKitService, popupService, navigationService, popupNavigation);
        }

        private static ChatPageViewModel GetNewChatPageViewModel(LMKitService lmKitService, ConversationListViewModel conversationListViewModel,
            ModelListViewModel modelListViewModel, IMaestroDatabase database,  ILLMFileManager llmFileManager, SettingsViewModel settingsViewModel)
        {
            var popupService = new Mock<IPopupService>().Object;
            var mainThread = new Mock<IMainThread>().Object;
            var appSettingsService = new Mock<IAppSettingsService>().Object;
            var logger = new Mock<ILogger<ChatPageViewModel>>().Object;
            var navigationService = new Mock<INavigationService>().Object;
            var popupNavigation = new Mock<IPopupNavigation>().Object;

            return new ChatPageViewModel(navigationService, popupService, popupNavigation, conversationListViewModel, 
                modelListViewModel, logger, database, lmKitService, llmFileManager, settingsViewModel);
        }
    }
}
