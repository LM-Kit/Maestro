using System.Collections;

namespace LMKitMaestro.Controls;

public partial class CustomCollectionView : ScrollView
{
    private VisualElement? _latestSelectedVisualElement;

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(CustomCollectionView));
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(CustomCollectionView));
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(CustomCollectionView), propertyChanged: OnSelectedItemPropertyChanged);
    public object? SelectedItem
    {
        get => (object?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly BindableProperty SpacingProperty = BindableProperty.Create(nameof(Spacing), typeof(double), typeof(CustomCollectionView));
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public static readonly BindableProperty ChildViewsProperty = BindableProperty.Create(nameof(ChildViews), typeof(IList<IView>), typeof(CustomCollectionView));
    public IList<IView> ChildViews
    {
        get => (IList<IView>)GetValue(ChildViewsProperty);
        set => SetValue(ChildViewsProperty, value);
    }

    public event EventHandler<EventArgs>? SelectionChanged;
    public event EventHandler<ElementEventArgs>? ChildViewAdded;

    public CustomCollectionView()
    {

        InitializeComponent();
    }

    private void OnRootLayoutChildAdded(object sender, ElementEventArgs e)
    {
        SetBinding(ChildViewsProperty, new Binding(nameof(VerticalStackLayout.Children), BindingMode.OneWay, source: Content));

        if (e.Element is View view)
        {
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer()
            {
                NumberOfTapsRequired = 1,
            };

            PointerGestureRecognizer pointerGestureRecognizer = new PointerGestureRecognizer();

            pointerGestureRecognizer.PointerEntered += OnPointerGestureRecognizePointerEntered;
            pointerGestureRecognizer.PointerExited += OnPointerGestureRecognizePointerExited;
            tapGestureRecognizer.Tapped += OnTapGestureRecognizerTapped;

            view.GestureRecognizers.Add(tapGestureRecognizer);
            view.GestureRecognizers.Add(pointerGestureRecognizer);
        }

        ChildViewAdded?.Invoke(this, e);
    }

    private void OnRootLayoutLoaded(object sender, EventArgs e)
    {
        if (SelectedItem != null)
        {
            foreach (var child in rootLayout.Children)
            {
                if (child is VisualElement visualElement)
                {
                    if (visualElement.BindingContext == SelectedItem)
                    {
                        SetSelectedElement(visualElement);
                    }
                    else
                    {
                        VisualStateManager.GoToState(visualElement, "_Normal");
                    }
                }
            }
        }
    }

    private void SetSelectedElement(VisualElement visualElement)
    {
        if (_latestSelectedVisualElement != null)
        {
            VisualStateManager.GoToState(_latestSelectedVisualElement, "_Normal");
        }

        _latestSelectedVisualElement = visualElement;
        VisualStateManager.GoToState(visualElement, "_Selected");
        SelectedItem = visualElement.BindingContext;
    }

    private void OnPointerGestureRecognizePointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is VisualElement visualElement)
        {
            bool isSelected = ((VisualElement)sender).BindingContext == SelectedItem;

            VisualStateManager.GoToState(visualElement, isSelected ? "_Selected" : "_Normal");
        }
    }

    private void OnPointerGestureRecognizePointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is VisualElement visualElement)
        {
            bool isSelected = ((VisualElement)sender).BindingContext == SelectedItem;

            if (!isSelected)
            {
                VisualStateManager.GoToState(visualElement, "_Hovered");
            }
        }
    }

    private void OnTapGestureRecognizerTapped(object? sender, TappedEventArgs e)
    {
        if (sender is VisualElement visualElement)
        {
            SetSelectedElement(visualElement);
        }
    }

    private static void OnSelectedItemPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var customCollectionView = (CustomCollectionView)bindable;

        if (customCollectionView._latestSelectedVisualElement != null)
        {
            VisualStateManager.GoToState(customCollectionView._latestSelectedVisualElement, "_Normal");
        }

        if (newValue != null)
        {
            foreach (var child in customCollectionView.rootLayout.Children)
            {
                if (child is VisualElement childVisualElement && childVisualElement.BindingContext == newValue)
                {
                    customCollectionView.SetSelectedElement(childVisualElement);
                    break;
                }
            }
        }

        customCollectionView.SelectionChanged?.Invoke(bindable, EventArgs.Empty);
    }
}