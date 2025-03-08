using LMKit.Maestro.Data;
using LMKit.Maestro.Helpers;
using LMKit.Maestro.Services;
using Maestro.Tests.Services;
using LMKit.Maestro.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using LMKit.Model;

namespace Maestro.Tests
{
    internal class MaestroTestsService
    {
        private Exception? _errorLoadingException;
        TaskCompletionSource<bool>? _modelLoadingTask;
        TaskCompletionSource<bool>? _modelUnloadedTask;
        TaskCompletionSource<bool>? _downloadProgressTask;

        public bool ProgressEventWasRaided { get; private set; }

        public IAppSettingsService AppSettingsService { get; } = new DummyAppSettingsService();
        public ILLMFileManager LLmFileManager { get; }
        public LMKitService LMKitService { get; } = new LMKitService();
        public IMaestroDatabase Database { get; } = new DummyMaestroDatabase();

        public SettingsViewModel SettingsViewModel { get; }
        public ConversationListViewModel ConversationListViewModel { get; }
        public ModelListViewModel ModelListViewModel { get; }
        public ChatPageViewModel ChatPageViewModel { get; }

        public MaestroTestsService()
        {
            LLmFileManager = new DummyLLmFileManager(AppSettingsService, Constants.HttpClient);
            SettingsViewModel = GetNewSettingsViewModel(LMKitService);
            ConversationListViewModel = GetNewConversationListViewModel(LMKitService, Database);
            ModelListViewModel = GetNewModelListViewModel(LMKitService, LLmFileManager);
            ChatPageViewModel = GetNewChatPageViewModel(LMKitService, ConversationListViewModel, ModelListViewModel, SettingsViewModel);
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

        public async Task<bool> StartModelDownload(ModelCard modelCard)
        {
            _downloadProgressTask = new TaskCompletionSource<bool>();
            LLmFileManager.ModelDownloadingProgressed += OnModelDownloadingProgressed;
            LLmFileManager.DownloadModel(modelCard);

            var result = await _downloadProgressTask.Task;
            return result;
        }
        public async Task<bool> LoadModel(Uri? modelUri = null)
        {
            if (modelUri == null)
            {
                modelUri = Constants.Model1;
            }

            _modelLoadingTask = new TaskCompletionSource<bool>();
            ProgressEventWasRaided = false;
            LMKitService.ModelLoadingProgressed += OnModelDownloadingProgressed;
            LMKitService.ModelLoaded += LMKitService_ModelLoadingCompleted;
            LMKitService.ModelLoadingFailed += LMKitService_ModelLoadingFailed;

            string? localFilePath =
                FileHelpers.GetModelFilePathFromUrl(modelUri, LMKitDefaultSettings.DefaultModelStorageDirectory);

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

        private void OnModelDownloadingProgressed(object? sender, EventArgs e)
        {
            var args = (LMKitService.ModelLoadingProgressedEventArgs)e;

            if (args.Progress == 0)
            {
                Trace.WriteLine("Downloading model...");
            }
            else if (args.Progress % 10 == 0)
            {
                Trace.WriteLine(args.Progress);
            }

            ProgressEventWasRaided = true;
            _downloadProgressTask!.SetResult(true);
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

        private static ConversationViewModel GetNewConversationViewModel(LMKitService lmKitService,
            IMaestroDatabase database)
        {
            var mainThread = new Mock<IMainThread>().Object;

            return new ConversationViewModel(lmKitService, database);
        }

        private static SettingsViewModel GetNewSettingsViewModel(LMKitService lmKitService)
        {
            var appSettingsService = new Mock<IAppSettingsService>().Object;

            return new SettingsViewModel(appSettingsService, lmKitService);
        }

        private static ConversationListViewModel GetNewConversationListViewModel(LMKitService lmKitService,
            IMaestroDatabase database)
        {
            var mainThread = new Mock<IMainThread>().Object;
            var appSettingsService = new Mock<IAppSettingsService>().Object;
            var logger = new Mock<ILogger<ConversationListViewModel>>().Object;

            return new ConversationListViewModel(mainThread, logger, database, lmKitService,
                appSettingsService);
        }

        private static ModelListViewModel GetNewModelListViewModel(LMKitService lmKitService,
            ILLMFileManager llmFileManager)
        {
            var launcher = new Mock<ILauncher>().Object;
            var snackbarService = new Mock<ISnackbarService>().Object;

            return new ModelListViewModel(llmFileManager, lmKitService, launcher, snackbarService);
        }

        private static ChatPageViewModel GetNewChatPageViewModel(LMKitService lmKitService,
            ConversationListViewModel conversationListViewModel,
            ModelListViewModel modelListViewModel, SettingsViewModel settingsViewModel)
        {
            var appSettingsService = new Mock<IAppSettingsService>().Object;
            var logger = new Mock<ILogger<ChatPageViewModel>>().Object;

            return new ChatPageViewModel(conversationListViewModel, modelListViewModel, lmKitService, settingsViewModel);
        }
    }
}