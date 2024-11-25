using LMKit.Maestro.ViewModels;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace LMKit.Maestro
{
    public partial class App : Application
    {
        private readonly AppShellViewModel _appShellViewModel;

        public App(AppShellViewModel appShellViewModel)
        {
            InitializeComponent();
            LMKit.Global.Runtime.Initialize();

            BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping("CustomBlazorWebView", (handler, view) =>
            {
#if WINDOWS
                // Setting background color of Blazor Web View to the page background color
                // to avoid visual white flash while the view is loading.
                if (App.Current != null && App.Current.Resources.TryGetValue("Background", out object value) && value is Color color)
                {
                    color.ToRgb(out byte r, out byte g, out byte b);

                    handler.PlatformView.DefaultBackgroundColor = new Windows.UI.Color()
                    {
                        A = 255,
                        R = r,
                        G = g,
                        B = b
                    };
                }
#endif
            });

            _appShellViewModel = appShellViewModel;
            MainPage = new AppShell(appShellViewModel);
        }

        protected override async void OnStart()
        {
            base.OnStart();

            Current!.UserAppTheme = AppTheme.Dark;
            await _appShellViewModel.Init();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = base.CreateWindow(activationState);

            window.Destroying += OnAppWindowDestroying;

            window.MinimumWidth = AppConstants.WindowMinimumWidth;
            window.MinimumHeight = AppConstants.WindowMinimumHeight;

            return window;
        }

        private void OnAppWindowDestroying(object? sender, EventArgs e)
        {
            _appShellViewModel.SaveAppSettings();
        }
    }
}
