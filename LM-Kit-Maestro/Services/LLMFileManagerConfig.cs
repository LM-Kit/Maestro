using LMKit.Maestro.Services;
using LMKit.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static MudBlazor.Colors;

public class LLMFileManagerConfig : INotifyPropertyChanged
{
    private List<ModelCapabilities> _filteredCapabilities = new() { ModelCapabilities.Chat, ModelCapabilities.Math, ModelCapabilities.CodeCompletion };
    public List<ModelCapabilities> FilteredCapabilities
    {
        get => _filteredCapabilities;
        set => SetProperty(ref _filteredCapabilities, value);
    }

    private bool _enableCustomModels = LMKitDefaultSettings.DefaultEnablePredefinedModels;
    public bool DefaultEnableCustomModels
    {
        get => _enableCustomModels;
        set => SetProperty(ref _enableCustomModels, value);
    }

    private bool _enablePredefinedModels = LMKitDefaultSettings.DefaultEnablePredefinedModels;
    public bool EnablePredefinedModels
    {
        get => _enablePredefinedModels;
        set => SetProperty(ref _enablePredefinedModels, value);
    }

    private bool _enableLowPerformanceModels = LMKitDefaultSettings.DefaultEnableLowPerformanceModels;
    public bool EnableLowPerformanceModels
    {
        get => _enableLowPerformanceModels;
        set => SetProperty(ref _enableLowPerformanceModels, value);
    }

    private string _modelsStorageDirectory = LMKitDefaultSettings.DefaultModelStorageDirectory;
    public string ModelsDirectory
    {
        get => _modelsStorageDirectory;
        set => SetProperty(ref _modelsStorageDirectory, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (propertyName == nameof(ModelsDirectory))
        {
            LMKit.Global.Configuration.ModelStorageDirectory = ModelsDirectory;
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetProperty<T>(ref T previousValue, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(previousValue, newValue))
        {
            previousValue = newValue;
            OnPropertyChanged(propertyName);
        }
    }
}
