using LMKit.Model;
using LMKit.TextGeneration.Sampling;
using System.ComponentModel;

namespace LMKit.Maestro.Services;

/// <summary>
/// This service is intended to be used as a singleton via Dependency Injection. 
/// Please register with <c>services.AddSingleton&lt;LMKitService&gt;()</c>.
/// </summary>
public partial class LMKitService : INotifyPropertyChanged
{
    private readonly LMKitServiceState _state;

    public LMKitConfig LMKitConfig => _state.Config;

    public LMKitChat Chat { get; }

    public LMKitModelLoadingState ModelLoadingState
    {
        get => _state.ModelLoadingState;
        set
        {
            if (_state.ModelLoadingState != value)
            {
                _state.ModelLoadingState = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModelLoadingState)));
            }
        }
    }

    public Uri? LoadedModelUri
    {
        get => _state.LoadedModelUri;
        set
        {
            if (_state.LoadedModelUri != value)
            {
                _state.LoadedModelUri = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadedModelUri)));
            }
        }
    }

    /// <summary>
    /// Whether the currently loaded model supports vision (image analysis).
    /// </summary>
    public bool SupportsVision => _state.LoadedModel != null && _state.LoadedModel.HasVision;

    public event NotifyModelStateChangedEventHandler? ModelLoadingProgressed;
    public event NotifyModelStateChangedEventHandler? ModelDownloadingProgressed;
    public event NotifyModelStateChangedEventHandler? ModelLoaded;
    public event NotifyModelStateChangedEventHandler? ModelLoadingFailed;
    public event NotifyModelStateChangedEventHandler? ModelUnloaded;

    public event PropertyChangedEventHandler? PropertyChanged;

    public delegate void NotifyModelStateChangedEventHandler(object? sender, NotifyModelStateChangedEventArgs notifyModelStateChangedEventArgs);


    public LMKitService()
    {
        _state = new LMKitServiceState();
        Chat = new LMKitChat(_state);
    }

    public void LoadModel(Uri modelUri, string? localFilePath = null)
    {
        if (_state.LoadedModel != null)
        {
            UnloadModel();
        }

        _state.Semaphore.Wait();
        LoadedModelUri = modelUri;
        ModelLoadingState = LMKitModelLoadingState.Loading;
        WasLoadingCancelled = false;

        var modelLoadingTask = new Task(() =>
        {
            bool modelLoadingSuccess;

            try
            {
                _state.LoadedModel = new LM(modelUri, downloadingProgress: OnModelDownloadingProgressed, loadingProgress: OnModelLoadingProgressed);

                modelLoadingSuccess = true;
            }
            catch (Exception exception)
            {
                modelLoadingSuccess = false;
                ModelLoadingFailed?.Invoke(this, new ModelLoadingFailedEventArgs(modelUri, exception));
            }
            finally
            {
                LoadedModelUri = null;
                _state.Semaphore.Release();
            }

            if (modelLoadingSuccess)
            {
                LMKitConfig.LoadedModelUri = modelUri!;

                ModelLoaded?.Invoke(this, new NotifyModelStateChangedEventArgs(LMKitConfig.LoadedModelUri));
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
        _state.Semaphore.Wait();

        var unloadedModelUri = LMKitConfig.LoadedModelUri!;

        Chat.TerminateChatService();

        if (_state.LoadedModel != null)
        {
            _state.LoadedModel.Dispose();
            _state.LoadedModel = null;
        }

        _state.Semaphore.Release();

        ModelLoadingState = LMKitModelLoadingState.Unloaded;
        LMKitConfig.LoadedModelUri = null;

        ModelUnloaded?.Invoke(this, new NotifyModelStateChangedEventArgs(unloadedModelUri));
    }

    private bool _cancelModelLoading;

    public bool WasLoadingCancelled { get; private set; }

    public void CancelModelLoading()
    {
        // Cancel if model is currently being loaded (includes downloading)
        if (ModelLoadingState == LMKitModelLoadingState.Loading)
        {
            _cancelModelLoading = true;
            WasLoadingCancelled = true;
        }
    }


    private bool OnModelLoadingProgressed(float progress)
    {
        if (_cancelModelLoading)
        {
            _cancelModelLoading = false;
            return false;
        }

        ModelLoadingProgressed?.Invoke(this, new ModelLoadingProgressedEventArgs(_state.LoadedModelUri!, progress));

        return true;
    }

    private bool OnModelDownloadingProgressed(string path, long? contentLength, long bytesRead)
    {
        if (_cancelModelLoading)
        {
            _cancelModelLoading = false;
            return false;
        }

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
                    //LocallyTypical = config.RandomSamplingConfig.LocallyTypical
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

            case SamplingMode.TopNSigma:
                return new TopNSigmaSampling()
                {
                    Temperature = config.TopNSigmaSamplingConfig.Temperature,
                    TopK = config.TopNSigmaSamplingConfig.TopK,
                    TopNSigma = config.TopNSigmaSamplingConfig.TopNSigma,
                };
        }
    }
}