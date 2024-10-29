using LMKitMaestro.ViewModels;
using LMKitMaestro.Services;

namespace LMKitMaestro.Views;

public partial class ModelSelectionButton : ContentView
{
    private bool _mustIgnoreNextStatefulContentViewTap;

    private ChatPageViewModel? _chatPageViewModel;

    public static readonly BindableProperty LoadingTextProperty = BindableProperty.Create(nameof(LoadingText), typeof(string), typeof(ModelSelectionButton));
    public string LoadingText
    {
        get => (string)GetValue(LoadingTextProperty);
        private set => SetValue(LoadingTextProperty, value);
    }

    public ModelSelectionButton()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is ChatPageViewModel chatPageViewModel)
        {
            chatPageViewModel.LmKitService.ModelLoadingProgressed += OnModelLoadingProgressed;
            chatPageViewModel.LmKitService.ModelLoadingCompleted += OnModelLoadingCompleted;
            _chatPageViewModel = chatPageViewModel;
        }
    }

    private void OnModelLoadingProgressed(object? sender, EventArgs e)
    {
        var modelLoadingProgressedEventArgs = (LMKitService.ModelLoadingProgressedEventArgs)e;

        if (modelLoadingProgressedEventArgs != null)
        {
            if (modelLoadingProgressedEventArgs.Progress == 1)
            {
                LoadingText = "Finishing up..."; // had to set this text directly in code-behind because DataTrigger with MultiBinding did not work.
            }
            else
            {
                LoadingText = "Loading model...";
            }
        }
    }

    private void OnModelLoadingCompleted(object? sender, LMKitService.NotifyModelStateChangedEventArgs notifyModelStateChangedEventArgs)
    {
        // Resetting default loading label.
        LoadingText = "Loading model...";
    }

    private void OnEjectModelButtonClicked(object sender, EventArgs e)
    {
        if (_chatPageViewModel != null)
        {
            // Workaround: StatefulContentView events get fired whenever one of its child fires an event
            // -> ignore next click event
            _mustIgnoreNextStatefulContentViewTap = true;

            if (_chatPageViewModel != null)
            {
                _chatPageViewModel.EjectModel();
            }
        }
    }

    private async void OnModelSelectionButtonClicked(object sender, EventArgs e)
    {
        if (!_mustIgnoreNextStatefulContentViewTap)
        {
            if (_chatPageViewModel != null)
            {
                ModelSelectionPopupViewModel modelSelectionPopupViewModel = new ModelSelectionPopupViewModel(_chatPageViewModel);

                var popup = new ModelSelectionPopup(_chatPageViewModel.PopupNavigation, modelSelectionPopupViewModel);

                await _chatPageViewModel.PopupNavigation.PushAsync(popup, true);
            }
        }

        _mustIgnoreNextStatefulContentViewTap &= false;
    }
}