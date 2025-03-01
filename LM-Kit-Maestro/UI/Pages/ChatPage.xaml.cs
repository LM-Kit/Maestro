using LMKit.Maestro.ViewModels;

namespace LMKit.Maestro.UI;

public partial class ChatPage : ContentPage
{
    private const int SidebarAnimationLength = 100;
    public static readonly Thickness ChatViewMargin = new Thickness(UIConstants.HeaderHorizontalMargin, 0, UIConstants.HeaderHorizontalMargin, 0);

    private readonly ChatPageViewModel _chatViewModel;

    public static readonly BindableProperty ShowSidebarTogglesProperty = BindableProperty.Create(nameof(ShowSidebarToggles), typeof(bool), typeof(ChatPage), propertyChanged: OnShowSidebarTogglesPropertyChanged);
    public bool ShowSidebarToggles
    {
        get => (bool)GetValue(ShowSidebarTogglesProperty);
        private set => SetValue(ShowSidebarTogglesProperty, value);
    }

    public static readonly BindableProperty ChatsSidebarWidthProperty = BindableProperty.Create(nameof(ChatsSidebarWidth), typeof(double), typeof(ChatPage));
    public double ChatsSidebarWidth
    {
        get => (double)GetValue(ChatsSidebarWidthProperty);
        private set => SetValue(ChatsSidebarWidthProperty, value);
    }

    public static readonly BindableProperty SettingsSidebarWidthProperty = BindableProperty.Create(nameof(SettingsSidebarWidth), typeof(double), typeof(ChatPage));
    public double SettingsSidebarWidth
    {
        get => (double)GetValue(SettingsSidebarWidthProperty);
        private set => SetValue(SettingsSidebarWidthProperty, value);
    }

    public static readonly BindableProperty ChatsSidebarAnimatingProperty = BindableProperty.Create(nameof(ChatsSidebarAnimating), typeof(bool), typeof(ChatPage));
    public bool ChatsSidebarAnimating
    {
        get => (bool)GetValue(ChatsSidebarAnimatingProperty);
        private set => SetValue(ChatsSidebarAnimatingProperty, value);
    }

    public static readonly BindableProperty SettingsSidebarAnimatingProperty = BindableProperty.Create(nameof(SettingsSidebarAnimating), typeof(bool), typeof(ChatPage));
    public bool SettingsSidebarAnimating
    {
        get => (bool)GetValue(SettingsSidebarAnimatingProperty);
        private set => SetValue(SettingsSidebarAnimatingProperty, value);
    }

    public ChatPage()
    {
        InitializeComponent();
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        ShowSidebarToggles = Width >= UIConstants.ChatWindowLayoutMinimumWidth;

        if (!ShowSidebarToggles)
        {
            _chatViewModel.ChatsSidebarIsToggled &= false;
            _chatViewModel.SettingsSidebarIsToggled &= false;
        }
    }

    private void OnChatPageViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (ShowSidebarToggles && (e.PropertyName == nameof(ChatPageViewModel.ChatsSidebarIsToggled) || e.PropertyName == nameof(ChatPageViewModel.SettingsSidebarIsToggled)))
        {
            if (e.PropertyName == nameof(ChatPageViewModel.ChatsSidebarIsToggled))
            {
                HandleChatsSidebarAnimation();
            }
            else if (e.PropertyName == nameof(ChatPageViewModel.SettingsSidebarIsToggled))
            {
                HandleSettingsSidebarAnimation();
            }
        }
    }

    private void OnChatSidebarAnimationComplete(double arg1, bool arg2)
    {
        ChatsSidebarAnimating = false;
    }

    private void OnSettingsSidebarAnimationComplete(double arg1, bool arg2)
    {
        SettingsSidebarAnimating = false;
    }

    private static void OnShowSidebarTogglesPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var chatPage = (ChatPage)bindable;

        chatPage.AbortAnimation("ChatsSidebarAnimation");
        chatPage.AbortAnimation("SettingsSidebarAnimation");

        chatPage.SettingsSidebarWidth = 0;
        chatPage.ChatsSidebarWidth = 0;
    }

    private async void HandleChatsSidebarAnimation()
    {
        Animation sidebarAnimation;
        if (_chatViewModel.ChatsSidebarIsToggled)
        {
            sidebarAnimation = new Animation(v => ChatsSidebarWidth = v, 0, UIConstants.ChatPageSidebarWidth);
        }
        else
        {
            sidebarAnimation = new Animation(v => ChatsSidebarWidth = v, UIConstants.ChatPageSidebarWidth, 0);
        }

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            ChatsSidebarAnimating = true;
            sidebarAnimation.Commit(this, "ChatsSidebarAnimation", 16, SidebarAnimationLength, Easing.Linear, finished: OnChatSidebarAnimationComplete);
        });
    }

    private async void HandleSettingsSidebarAnimation()
    {
        Animation sidebarAnimation;

        if (_chatViewModel.SettingsSidebarIsToggled)
        {
            SettingsSidebarAnimating = true;

            sidebarAnimation = new Animation(v => SettingsSidebarWidth = v, 0, UIConstants.ChatPageSidebarWidth);
        }
        else
        {
            sidebarAnimation = new Animation(v => SettingsSidebarWidth = v, UIConstants.ChatPageSidebarWidth, 0);
        }

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            SettingsSidebarAnimating = true;
            sidebarAnimation.Commit(this, "SettingsSidebarAnimation", 16, SidebarAnimationLength, Easing.Linear, finished: OnSettingsSidebarAnimationComplete);
        });
    }

    //public static Task<bool> ColorTo(this VisualElement self, Thickness fromMargin, Thickness toMargin, Action<Thickness> callback, uint length = 250, Easing easing = null)
    //{
    //    Func<double, Thickness> transform = (t) => new Thickness(fromMargin.Left + t, fromMargin.Top, fromMargin.Right, fromMargin.Bottom);

    //    return MarginAnimation(self, "Margin", transform, callback, length, easing);
    //}

    //public static void CancelAnimation(this VisualElement self)
    //{
    //    self.AbortAnimation("ColorTo");
    //}

    //static Task<bool> MarginAnimation(VisualElement element, string name, Func<double, Thickness> transform, Action<Thickness> callback, uint length, Easing easing)
    //{
    //    easing = easing ?? Easing.Linear;
    //    var taskCompletionSource = new TaskCompletionSource<bool>();

    //    element.Animate<Thickness>(name, transform, callback, 16, length, easing, (v, c) => taskCompletionSource.SetResult(c));
    //    return taskCompletionSource.Task;
    //}
}