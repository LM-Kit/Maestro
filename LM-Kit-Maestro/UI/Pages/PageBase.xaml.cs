namespace LMKit.Maestro.UI.Pages;

public partial class PageBase : ContentPage
{
    public static readonly BindableProperty? HeaderProperty = BindableProperty.Create(nameof(Header), typeof(View), typeof(PageBase));
    public View? Header
    {
        get => (View)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly BindableProperty BodyProperty = BindableProperty.Create(nameof(Body), typeof(View), typeof(PageBase));
    public View Body
    {
        get => (View)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public PageBase()
	{
		InitializeComponent();
	}
}