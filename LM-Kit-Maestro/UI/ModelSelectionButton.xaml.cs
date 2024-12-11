using LMKit.Maestro.ViewModels;
using LMKit.Maestro.Services;

namespace LMKit.Maestro.UI;

public partial class ModelSelectionButton : ContentView
{
    private bool _mustIgnoreNextStatefulContentViewTap;

    private ModelListViewModel? _modelListViewModel;

    public static readonly BindableProperty IsHoverdProperty = BindableProperty.Create(nameof(IsHovered), typeof(bool), typeof(ModelSelectionButton));
    public bool IsHovered
    {
        get => (bool)GetValue(IsHoverdProperty);
        private set => SetValue(IsHoverdProperty, value);
    }

    public static readonly BindableProperty LoadingTextProperty = BindableProperty.Create(nameof(LoadingText), typeof(string), typeof(ModelSelectionButton));
    public string LoadingText
    {
        get => (string)GetValue(LoadingTextProperty);
        private set => SetValue(LoadingTextProperty, value);
    }

    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(ModelSelectionButton));
    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        private set => SetValue(BorderColorProperty, value);
    }

    public ModelSelectionButton()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is ModelListViewModel modelListViewModel)
        {
            modelListViewModel.LMKitService.ModelLoadingProgressed += OnModelLoadingProgressed;
            modelListViewModel.LMKitService.ModelLoadingCompleted += OnModelLoadingCompleted;
            _modelListViewModel = modelListViewModel;
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
        if (_modelListViewModel != null)
        {
            // Workaround: StatefulContentView events get fired whenever one of its child fires an event
            // -> ignore next click event
            _mustIgnoreNextStatefulContentViewTap = true;

            if (_modelListViewModel != null)
            {
                _modelListViewModel.EjectModel();
            }
        }
    }

    private async void OnModelSelectionButtonClicked(object sender, EventArgs e)
    {
        if (!_mustIgnoreNextStatefulContentViewTap)
        {
            if (_modelListViewModel != null)
            {
                ModelSelectionPopupViewModel modelSelectionPopupViewModel = new ModelSelectionPopupViewModel(_modelListViewModel);

                var popup = new ModelSelectionPopup(_modelListViewModel.PopupNavigation, modelSelectionPopupViewModel);

                await _modelListViewModel.PopupNavigation.PushAsync(popup, true);
            }
        }

        _mustIgnoreNextStatefulContentViewTap &= false;
    }

    private void OnModelSelectionButtonHovered(object sender, EventArgs e)
    {
        IsHovered = true;
    }

    private void OnModelSelectionButtonHoveredExited(object sender, EventArgs e)
    {
        IsHovered = false;
    }
}