using LMKit.Maestro.ViewModels;
using LMKit.Maestro.Services;

namespace LMKit.Maestro.UI;

public partial class ChatSettingsView : ContentView
{
    private SettingsViewModel? _settingsViewModel;

    public static readonly BindableProperty MaxCompletionTokensTextProperty = BindableProperty.Create(nameof(MaxCompletionTokensText), typeof(string), typeof(ChatSettingsView));
    public string MaxCompletionTokensText
    {
        get => (string)GetValue(MaxCompletionTokensTextProperty);
        set => SetValue(MaxCompletionTokensTextProperty, value);
    }

    public ChatSettingsView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is SettingsViewModel settingsViewModel)
        {
            _settingsViewModel = settingsViewModel;
            updateMaxCompletionTokensText();
            _settingsViewModel.PropertyChanged += OnSettingsViewModelPropertyChanged;
        }
    }

    private void OnSettingsViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.MaximumCompletionTokens))
        {
            updateMaxCompletionTokensText();
        }
    }

    private void OnSystemPromptUnfocused(object sender, FocusEventArgs e)
    {
        if (_settingsViewModel != null && string.IsNullOrWhiteSpace(_settingsViewModel.SystemPrompt))
        {
            _settingsViewModel.SystemPrompt = LMKitDefaultSettings.DefaultSystemPrompt;
        }
    }

    private void OnMaxCompletionTokensEntryUnfocused(object sender, FocusEventArgs e)
    {
        if (int.TryParse(MaxCompletionTokensText, out int maxCompletionTokens))
        {
            _settingsViewModel!.MaximumCompletionTokens = Math.Max(maxCompletionTokens, 1);
        }
        else
        {
            updateMaxCompletionTokensText();
        }
    }

    private void updateMaxCompletionTokensText()
    {//Loïc: can't we bind _settingsViewModel!.MaximumCompletionTokens to MaxCompletionTokensText??
        MaxCompletionTokensText = _settingsViewModel!.MaximumCompletionTokens.ToString();
    }
}