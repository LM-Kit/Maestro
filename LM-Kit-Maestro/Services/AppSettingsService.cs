using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace LMKit.Maestro.Services;

public partial class AppSettingsService : ObservableObject, IAppSettingsService
{
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
        }
    }

    public RandomSamplingConfig RandomSamplingConfig
    {
        get
        {
            RandomSamplingConfig? randomSamplingConfig = null;

            try
            {
                string? json = Settings.Get(nameof(RandomSamplingConfig), default(string?));

                if (!string.IsNullOrEmpty(json))
                {
                    randomSamplingConfig = JsonSerializer.Deserialize<RandomSamplingConfig>(json);
                }
            }
            catch
            {
            }

            return randomSamplingConfig != null ? randomSamplingConfig : new RandomSamplingConfig();
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
                OnPropertyChanged();
            }
        }
    }

    public Mirostat2SamplingConfig Mirostat2SamplingConfig
    {
        get
        {
            Mirostat2SamplingConfig? Mirostat2SamplingConfig = null;

            try
            {
                string? json = Settings.Get(nameof(Mirostat2SamplingConfig), default(string?));

                if (!string.IsNullOrEmpty(json))
                {
                    Mirostat2SamplingConfig = JsonSerializer.Deserialize<Mirostat2SamplingConfig>(json);
                }
            }
            catch
            {
            }

            return Mirostat2SamplingConfig != null ? Mirostat2SamplingConfig : new Mirostat2SamplingConfig();
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
                OnPropertyChanged();
            }
        }
    }
}