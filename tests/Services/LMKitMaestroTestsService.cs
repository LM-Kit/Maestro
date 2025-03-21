using LMKit.Maestro.Data;
using LMKit.Maestro.Helpers;
using LMKit.Maestro.Services;
using Maestro.Tests.Services;
using LMKit.Maestro.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;

namespace Maestro.Tests
{
    internal class MaestroTestsService
    {
        public static readonly Uri Model1 =
            new(
                @"https://huggingface.co/lm-kit/llama-3.2-1b-instruct.gguf/resolve/main/Llama-3.2-1B-Instruct-Q4_K_M.gguf?download=true");

        public static readonly Uri Model2 =
            new(
                @"https://huggingface.co/lm-kit/qwen-2.5-0.5b-instruct-gguf/resolve/main/Qwen-2.5-0.5B-Instruct-Q4_K_M.gguf?download=true");

        public static readonly Uri TranslationModel =
            new Uri(
                @"https://huggingface.co/lm-kit/falcon-3-10.3b-instruct-gguf/resolve/main/Falcon3-10B-Instruct-q4_k_m.gguf?download=true");

        private Exception? _errorLoadingException;
        TaskCompletionSource<bool>? _modelLoadingTask;
        TaskCompletionSource<bool>? _modelUnloadedTask;

        public bool ProgressEventWasRaided { get; private set; }

        public IAppSettingsService AppSettingsService = new DummyAppSettingsService();
        public ILLMFileManager LLmFileManager { get; } = new DummyLLmFileManager();
        public LMKitService LMKitService { get; } = new LMKitService();
        public IMaestroDatabase Database { get; } = new DummyMaestroDatabase();

        public ChatSettingsViewModel ChatSettingsViewModel { get; }
        public ConversationListViewModel ConversationListViewModel { get; }
        public ModelListViewModel ModelListViewModel { get; }
        public ChatPageViewModel ChatPageViewModel { get; }
        public ModelsSettingsViewModel ModelsSettingsViewModel { get; }

        public MaestroTestsService()
        {
            var mainThread = new Mock<IMainThread>().Object;

            ChatSettingsViewModel = new ChatSettingsViewModel(AppSettingsService, LMKitService);
            ModelsSettingsViewModel = new ModelsSettingsViewModel(AppSettingsService, LLmFileManager);
            ConversationListViewModel = new ConversationListViewModel(mainThread, new Mock<ILogger<ConversationListViewModel>>().Object, Database, LMKitService, AppSettingsService);
            ModelListViewModel = new ModelListViewModel(ModelsSettingsViewModel, LLmFileManager, LMKitService, new Mock<ILauncher>().Object, new Mock<ISnackbarService>().Object);
            ChatPageViewModel = new ChatPageViewModel(ConversationListViewModel, ModelListViewModel, LMKitService, ChatSettingsViewModel);
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

        private void LMKitService_ModelLoadingProgressed(object? sender, EventArgs e)
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
    }
}