using Mopups.Interfaces;

namespace LMKit.Maestro.UI;

public partial class PopupView : PopupBase
{
    public static readonly BindableProperty ShowBackgroundOverlayProperty = BindableProperty.Create(nameof(ShowBackgroundOverlay), typeof(bool), typeof(PopupView));
    public bool ShowBackgroundOverlay
    {
        get => (bool)GetValue(ShowBackgroundOverlayProperty);
        set => SetValue(ShowBackgroundOverlayProperty, value);
    }

    public static readonly BindableProperty BodyProperty = BindableProperty.Create(nameof(Body), typeof(View), typeof(PopupView), propertyChanged: OnBodyPropertyChanged);
    public View Body
    {
        get => (View)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public static readonly BindableProperty BodyVerticalOptionsProperty = BindableProperty.Create(nameof(BodyVerticalOptions), typeof(LayoutOptions), typeof(PopupView), defaultValue: LayoutOptions.Center);
    public LayoutOptions BodyVerticalOptions
    {
        get => (LayoutOptions)GetValue(BodyVerticalOptionsProperty);
        set => SetValue(BodyVerticalOptionsProperty, value);
    }

    public static readonly BindableProperty BodyMarginProperty = BindableProperty.Create(nameof(BodyMargin), typeof(Thickness), typeof(PopupView));
    public Thickness BodyMargin
    {
        get => (Thickness)GetValue(BodyMarginProperty);
        set => SetValue(BodyMarginProperty, value);
    }

    public static readonly BindableProperty ShowOkButtonProperty = BindableProperty.Create(nameof(ShowOkButton), typeof(bool), typeof(PopupView));
    public bool ShowOkButton
    {
        get => (bool)GetValue(ShowOkButtonProperty);
        set => SetValue(ShowOkButtonProperty, value);
    }

    public static readonly BindableProperty ShowCloseButtonProperty = BindableProperty.Create(nameof(ShowCloseButton), typeof(bool), typeof(PopupView));
    public bool ShowCloseButton
    {
        get => (bool)GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    public PopupView(IPopupNavigation popupNavigation) : base(popupNavigation)
    {
        InitializeComponent();
    }

    async void OnCloseButtonClicked(object? sender, EventArgs e) => await Dismiss();

    private async void OnPopupBackgroundTapped(object sender, TappedEventArgs e) => await Dismiss();

    private static void OnBodyPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        //if (bindable is PopupView PopupView)
        //{
        //   PopupView.co
        //}
    }
}