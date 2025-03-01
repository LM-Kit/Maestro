#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
#endif


using LMKit.Maestro.UI;
using LMKit.Maestro.ViewModels;
using Microsoft.AspNetCore.Components.WebView.Maui;
using LMKit.Maestro.UI.Pages;
#if MACCATALYST
using UIKit;
using WebKit;
#endif

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace LMKit.Maestro
{
    public partial class App : Application
    {
        private readonly AppShellViewModel _appShellViewModel;

        public App(AppShellViewModel appShellViewModel)
        {
            InitializeComponent();

            Task.Run(() => Global.Runtime.Initialize()); //Initialize LM-Kit in the background to avoid blocking UI initialization.

            BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping("CustomBlazorWebView", (handler, view) =>
            {
                // Setting background color of Blazor Web View to the page background color
                // to avoid visual white flash while the view is loading.
                if (App.Current != null && App.Current.Resources.TryGetValue("Background", out object value) && value is Color color)
                {
                    color.ToRgb(out byte r, out byte g, out byte b);
#if WINDOWS

                    handler.PlatformView.DefaultBackgroundColor = new Windows.UI.Color()
                    {
                        A = 255,
                        R = r,
                        G = g,
                        B = b
                    };
#elif MACCATALYST
                  if (handler.PlatformView is WKWebView wv)
                  {
                      wv.Opaque = false;
                      wv.BackgroundColor = UIColor.Clear;
                  }
#endif
                }
            });

            _appShellViewModel = appShellViewModel;
        }

        protected override async void OnStart()
        {
            base.OnStart();

            Current!.UserAppTheme = AppTheme.Dark;
            await _appShellViewModel.Init();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(new MainPage());

#if WINDOWS
            window.HandlerChanged += (sender, args) =>
            {
                var nativeWindow = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
                if (nativeWindow != null)
                {
                    CustomizeTitleBar(nativeWindow);
                }
            };
#endif

            window.Destroying += OnAppWindowDestroying;

            window.MinimumWidth = UIConstants.WindowMinimumWidth;
            window.MinimumHeight = UIConstants.WindowMinimumHeight;

            return window;
        }

        private void OnAppWindowDestroying(object? sender, EventArgs e)
        {
            _appShellViewModel.SaveAppSettings();
        }

#if WINDOWS
        private void CustomizeTitleBar(Microsoft.UI.Xaml.Window nativeWindow)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow is not null && AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = appWindow.TitleBar;

                // Set the minimize, maximize, and close button icon colors
                titleBar.ButtonForegroundColor = Microsoft.UI.Colors.White;
                titleBar.ButtonHoverForegroundColor = Microsoft.UI.Colors.White;
                titleBar.ButtonPressedForegroundColor = Microsoft.UI.Colors.White;
                titleBar.ButtonInactiveForegroundColor = Microsoft.UI.Colors.White;

                //  Set background colors for different states
                titleBar.ButtonBackgroundColor = ColorHelper.FromArgb(255, 81, 43, 212);
                titleBar.ButtonHoverBackgroundColor = ColorHelper.FromArgb(255, 100, 60, 230);
                titleBar.ButtonPressedBackgroundColor = ColorHelper.FromArgb(255, 60, 30, 150);
                titleBar.ButtonInactiveBackgroundColor = ColorHelper.FromArgb(255, 81, 43, 212);
            }
        }
#endif
    }
}
