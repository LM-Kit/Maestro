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
public partial class LMKitService
{
    private readonly LMKitServiceState _state;

    public LMKitConfig LMKitConfig => _state.Config;

    public LMKitTranslation Translation { get; }

    public LMKitChat Chat { get; }

    public event NotifyModelStateChangedEventHandler? ModelLoadingProgressed;
    public event NotifyModelStateChangedEventHandler? ModelDownloadingProgressed;
    public event NotifyModelStateChangedEventHandler? ModelLoaded;
    public event NotifyModelStateChangedEventHandler? ModelLoadingFailed;
    public event NotifyModelStateChangedEventHandler? ModelUnloaded;

    public delegate void NotifyModelStateChangedEventHandler(object? sender, NotifyModelStateChangedEventArgs notifyModelStateChangedEventArgs);

    public LMKitModelLoadingState ModelLoadingState => _state.ModelLoadingState;

    public LMKitService()
    {
        _state = new LMKitServiceState();
        Chat = new LMKitChat(_state);
        Translation = new LMKitTranslation(_state);
    }

    public void LoadModel(Uri fileUri, string? localFilePath = null)
    {
        if (_state.LoadedModel != null)
        {
            UnloadModel();
        }

        _state.Semaphore.Wait();
        _state.LoadedModelUri = fileUri;
        _state.ModelLoadingState = LMKitModelLoadingState.Loading;

        var modelLoadingTask = new Task(() =>
        {
            bool modelLoadingSuccess;

            try
            {
                _state.LoadedModel = new LM(fileUri, downloadingProgress: OnModelDownloadingProgressed, loadingProgress: OnModelLoadingProgressed);

                modelLoadingSuccess = true;
            }
            catch (Exception exception)
            {
                modelLoadingSuccess = false;
                ModelLoadingFailed?.Invoke(this, new ModelLoadingFailedEventArgs(fileUri, exception));
            }
            finally
            {
                _state.LoadedModelUri = null;
                _state.Semaphore.Release();
            }

            if (modelLoadingSuccess)
            {
                LMKitConfig.LoadedModelUri = fileUri!;

                ModelLoaded?.Invoke(this, new NotifyModelStateChangedEventArgs(LMKitConfig.LoadedModelUri));
                _state.ModelLoadingState = LMKitModelLoadingState.Loaded;
            }
            else
            {
                _state.ModelLoadingState = LMKitModelLoadingState.Unloaded;
            }

        });

        modelLoadingTask.Start();
    }

    public void UnloadModel()
    {
        // Ensuring we don't clean things up while a model is already being loaded,
        // or while the currently loaded model instance should not be touched
        // (while we are getting Lm-Kit objects ready to process a newly submitted prompt for instance).
        _state.Semaphore.Wait();

        var unloadedModelUri = LMKitConfig.LoadedModelUri!;

        Chat.TerminateChatService();

        if (_state.LoadedModel != null)
        {
            _state.LoadedModel.Dispose();
            _state.LoadedModel = null;
        }

        _state.Semaphore.Release();

        _state.ModelLoadingState = LMKitModelLoadingState.Unloaded;
        LMKitConfig.LoadedModelUri = null;

        ModelUnloaded?.Invoke(this, new NotifyModelStateChangedEventArgs(unloadedModelUri));
    }



    private bool OnModelLoadingProgressed(float progress)
    {
        ModelLoadingProgressed?.Invoke(this, new ModelLoadingProgressedEventArgs(_state.LoadedModelUri!, progress));

        return true;
    }

    private bool OnModelDownloadingProgressed(string path, long? contentLength, long bytesRead)
    {
        ModelDownloadingProgressed?.Invoke(this, new ModelDownloadingProgressedEventArgs(_state.LoadedModelUri!, path, contentLength, bytesRead));

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