using CommunityToolkit.Mvvm.ComponentModel;
using LMKit.TextGeneration.Chat;
using System.ComponentModel;
using System.Text.Json;

namespace LMKit.Maestro.Services;

public partial class AppSettingsService : INotifyPropertyChanged, IAppSettingsService
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected IPreferences Settings { get; }

    public AppSettingsService(IPreferences settings)
    {
        Settings = settings;
    }

    public Uri? LastLoadedModelUri
    {
        get
        {
            string? uriString = Settings.Get(nameof(LastLoadedModelUri), default(string?));

            if (!string.IsNullOrWhiteSpace(uriString))
            {
                return new Uri(uriString);
            }
            else
            {
                return null;
            }
        }
        set
        {
            Settings.Set(nameof(LastLoadedModelUri), value?.ToString());
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastLoadedModelUri)));
        }
    }

    public string ModelStorageDirectory
    {
        get
        {
            string directory = Settings.Get(nameof(ModelStorageDirectory), LMKitDefaultSettings.DefaultModelStorageDirectory);

            if (directory != LMKit.Global.Configuration.ModelStorageDirectory)
            {
                LMKit.Global.Configuration.ModelStorageDirectory = directory;
            }

            return directory;
        }
        set
        {
            Settings.Set(nameof(ModelStorageDirectory), value);
            LMKit.Global.Configuration.ModelStorageDirectory = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModelStorageDirectory)));
        }
    }

    public bool EnableLowPerformanceModels
    {
        get
        {
            return Settings.Get(nameof(EnableLowPerformanceModels), LMKitDefaultSettings.DefaultEnableLowPerformanceModels);
        }
        set
        {
            Settings.Set(nameof(EnableLowPerformanceModels), value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableLowPerformanceModels)));
        }
    }

    public bool EnablePredefinedModels
    {
        get
        {
            return Settings.Get(nameof(EnablePredefinedModels), LMKitDefaultSettings.DefaultEnablePredefinedModels);
        }
        set
        {
            Settings.Set(nameof(EnablePredefinedModels), value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnablePredefinedModels)));
        }
    }

    public string SystemPrompt
    {
        get
        {
            return Settings.Get(nameof(SystemPrompt), LMKitDefaultSettings.DefaultSystemPrompt);
        }
        set
        {
            Settings.Set(nameof(SystemPrompt), value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SystemPrompt)));
        }
    }

    public int MaximumCompletionTokens
    {
        get
        {
            return Settings.Get(nameof(MaximumCompletionTokens), LMKitDefaultSettings.DefaultMaximumCompletionTokens);
        }
        set
        {
            Settings.Set(nameof(MaximumCompletionTokens), value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaximumCompletionTokens)));
        }
    }

    public int RequestTimeout
    {
        get
        {
            return Settings.Get(nameof(RequestTimeout), LMKitDefaultSettings.DefaultRequestTimeout);
        }
        set
        {
            Settings.Set(nameof(RequestTimeout), value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RequestTimeout)));
        }
    }

    public int ContextSize
    {
        get
        {
            return Settings.Get(nameof(ContextSize), LMKitDefaultSettings.DefaultContextSize);
        }
        set
        {
            Settings.Set(nameof(ContextSize), value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContextSize)));
        }
    }

    public SamplingMode SamplingMode
    {
        get
        {
            return (SamplingMode)Settings.Get(nameof(SamplingMode), (int)LMKitDefaultSettings.DefaultSamplingMode);
        }
        set
        {
            Settings.Set(nameof(SamplingMode), (int)value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SamplingMode)));
        }
    }

    public RandomSamplingConfig RandomSamplingConfig
    {
        get
        {
            RandomSamplingConfig? samplingConfig = null;

            try
            {
                string? json = Settings.Get(nameof(RandomSamplingConfig), default(string?));

                if (!string.IsNullOrEmpty(json))
                {
                    samplingConfig = JsonSerializer.Deserialize<RandomSamplingConfig>(json);
                }
            }
            catch
            {
            }

            return samplingConfig != null ? samplingConfig : new RandomSamplingConfig();
        }
        set
        {
            string? json;

            try
            {
                json = JsonSerializer.Serialize(value);
            }
            catch
            {
                json = null;
            }

            if (!string.IsNullOrEmpty(json))
            {
                Settings.Set(nameof(RandomSamplingConfig), json);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RandomSamplingConfig)));
            }
        }
    }

    public TopNSigmaSamplingConfig TopNSigmaSamplingConfig
    {
        get
        {
            TopNSigmaSamplingConfig? samplingConfig = null;

            try
            {
                string? json = Settings.Get(nameof(TopNSigmaSamplingConfig), default(string?));

                if (!string.IsNullOrEmpty(json))
                {
                    samplingConfig = JsonSerializer.Deserialize<TopNSigmaSamplingConfig>(json);
                }
            }
            catch
            {
            }

            return samplingConfig != null ? samplingConfig : new TopNSigmaSamplingConfig();
        }
        set
        {
            string? json;

            try
            {
                json = JsonSerializer.Serialize(value);
            }
            catch
            {
                json = null;
            }

            if (!string.IsNullOrEmpty(json))
            {
                Settings.Set(nameof(TopNSigmaSamplingConfig), json);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TopNSigmaSamplingConfig)));
            }
        }
    }

    public Mirostat2SamplingConfig Mirostat2SamplingConfig
    {
        get
        {
            Mirostat2SamplingConfig? samplingConfig = null;

            try
            {
                string? json = Settings.Get(nameof(samplingConfig), default(string?));

                if (!string.IsNullOrEmpty(json))
                {
                    samplingConfig = JsonSerializer.Deserialize<Mirostat2SamplingConfig>(json);
                }
            }
            catch
            {
            }

            return samplingConfig != null ? samplingConfig : new Mirostat2SamplingConfig();
        }
        set
        {
            string? json;

            try
            {
                json = JsonSerializer.Serialize(value);
            }
            catch
            {
                json = null;
            }

            if (!string.IsNullOrEmpty(json))
            {
                Settings.Set(nameof(Mirostat2SamplingConfig), json);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Mirostat2SamplingConfig)));
            }
        }
    }
}