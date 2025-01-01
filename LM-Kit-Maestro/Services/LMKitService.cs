using LMKit.Model;
using LMKit.TextGeneration;
using LMKit.TextGeneration.Chat;
using LMKit.TextGeneration.Sampling;
using LMKit.Translation;
using System.ComponentModel;

namespace LMKit.Maestro.Services;

/// <summary>
/// This service is intended to be used as a singleton via Dependency Injection. 
/// Please register with <c>services.AddSingleton&lt;LMKitService&gt;()</c>.
/// </summary>
public partial class LMKitService : INotifyPropertyChanged
{
    private readonly SemaphoreSlim _lmKitServiceSemaphore = new SemaphoreSlim(1);

    private static Uri? _currentlyLoadingModelUri;
    private static Conversation? _lastConversationUsed = null;
    private static LM? _model;
    private static MultiTurnConversation? _multiTurnConversation;

    public LMKitConfig LMKitConfig { get; } = new LMKitConfig();

    public LMKitTranslation Translation { get; }

    public LMKitChat Chat { get; }

    public event NotifyModelStateChangedEventHandler? ModelLoadingProgressed;
    public event NotifyModelStateChangedEventHandler? ModelDownloadingProgressed;
    public event NotifyModelStateChangedEventHandler? ModelLoadingCompleted;
    public event NotifyModelStateChangedEventHandler? ModelLoadingFailed;
    public event NotifyModelStateChangedEventHandler? ModelUnloaded;
    public event PropertyChangedEventHandler? PropertyChanged;

    public delegate void NotifyModelStateChangedEventHandler(object? sender, NotifyModelStateChangedEventArgs notifyModelStateChangedEventArgs);

    private LMKitModelLoadingState _modelLoadingState;
    public LMKitModelLoadingState ModelLoadingState
    {
        get => _modelLoadingState;
        set
        {
            _modelLoadingState = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModelLoadingState)));
        }
    }

    public LMKitService()
    {
        Chat = new LMKitChat(LMKitConfig);
        Translation = new LMKitTranslation(LMKitConfig);
    }

    public void LoadModel(Uri fileUri, string? localFilePath = null)
    {
        if (_model != null)
        {
            UnloadModel();
        }

        _lmKitServiceSemaphore.Wait();
        _currentlyLoadingModelUri = fileUri;
        ModelLoadingState = LMKitModelLoadingState.Loading;

        var modelLoadingTask = new Task(() =>
        {
            bool modelLoadingSuccess;

            try
            {
                _model = new LM(fileUri, downloadingProgress: OnModelDownloadingProgressed, loadingProgress: OnModelLoadingProgressed);

                modelLoadingSuccess = true;
            }
            catch (Exception exception)
            {
                modelLoadingSuccess = false;
                ModelLoadingFailed?.Invoke(this, new ModelLoadingFailedEventArgs(fileUri, exception));
            }
            finally
            {
                _currentlyLoadingModelUri = null;
                _lmKitServiceSemaphore.Release();
            }

            if (modelLoadingSuccess)
            {
                LMKitConfig.LoadedModelUri = fileUri!;

                ModelLoadingCompleted?.Invoke(this, new NotifyModelStateChangedEventArgs(LMKitConfig.LoadedModelUri));
                ModelLoadingState = LMKitModelLoadingState.Loaded;
            }
            else
            {
                ModelLoadingState = LMKitModelLoadingState.Unloaded;
            }

        });

        modelLoadingTask.Start();
    }

    public void UnloadModel()
    {
        // Ensuring we don't clean things up while a model is already being loaded,
        // or while the currently loaded model instance should not be touched
        // (while we are getting Lm-Kit objects ready to process a newly submitted prompt for instance).
        _lmKitServiceSemaphore.Wait();

        var unloadedModelUri = LMKitConfig.LoadedModelUri!;

        Chat.CancelAllPrompts();

        if (_multiTurnConversation != null)
        {
            _multiTurnConversation.Dispose();
            _multiTurnConversation = null;
        }

        if (_model != null)
        {
            _model.Dispose();
            _model = null;
        }

        _lmKitServiceSemaphore.Release();

        _lastConversationUsed = null;
        ModelLoadingState = LMKitModelLoadingState.Unloaded;
        LMKitConfig.LoadedModelUri = null;

        ModelUnloaded?.Invoke(this, new NotifyModelStateChangedEventArgs(unloadedModelUri));
    }



    private bool OnModelLoadingProgressed(float progress)
    {
        ModelLoadingProgressed?.Invoke(this, new ModelLoadingProgressedEventArgs(_currentlyLoadingModelUri!, progress));

        return true;
    }

    private bool OnModelDownloadingProgressed(string path, long? contentLength, long bytesRead)
    {
        ModelDownloadingProgressed?.Invoke(this, new ModelDownloadingProgressedEventArgs(_currentlyLoadingModelUri!, path, contentLength, bytesRead));

        return true;
    }

    private static TokenSampling GetTokenSampling(LMKitConfig config)
    {
        switch (config.SamplingMode)
        {
            default:
            case SamplingMode.Random:
                return new RandomSampling()
                {
                    Temperature = config.RandomSamplingConfig.Temperature,
                    DynamicTemperatureRange = config.RandomSamplingConfig.DynamicTemperatureRange,
                    TopP = config.RandomSamplingConfig.TopP,
                    TopK = config.RandomSamplingConfig.TopK,
                    MinP = config.RandomSamplingConfig.MinP,
                    LocallyTypical = config.RandomSamplingConfig.LocallyTypical
                };

            case SamplingMode.Greedy:
                return new GreedyDecoding();

            case SamplingMode.Mirostat2:
                return new Mirostat2Sampling()
                {
                    Temperature = config.Mirostat2SamplingConfig.Temperature,
                    LearningRate = config.Mirostat2SamplingConfig.LearningRate,
                    TargetEntropy = config.Mirostat2SamplingConfig.TargetEntropy
                };
        }
    }
}