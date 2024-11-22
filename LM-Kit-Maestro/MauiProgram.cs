using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using LMKitMaestro.UI;
using LMKitMaestro.ViewModels;
using LMKitMaestro.Services;
using UraniumUI;
using LMKitMaestro.Data;
using Plainer.Maui;
using SimpleToolkit.SimpleShell;
using LMKitMaestro.Handlers;
using MetroLog.MicrosoftExtensions;
using MetroLog.Operators;
using Majorsoft.Blazor.Components.Common.JsInterop;
using Mopups.Hosting;
using Mopups.Interfaces;
using Mopups.Services;

namespace LMKitMaestro
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseSimpleShell()
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .ConfigureMopups()
                .ConfigureFonts(fonts =>
                {
                    // Roboto
                    fonts.AddFont("Roboto-Thin.ttf", "RobotoThin");
                    fonts.AddFont("Roboto-Light.ttf", "RobotoLight");
                    fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
                    fonts.AddFont("Roboto-Italic.ttf", "RobotoItalic");
                    fonts.AddFont("Roboto-Bold.ttf", "RobotoBold");

                    // Segoe
                    fonts.AddFont("Segoe UI Light.ttf", "SegoeLight");
                    fonts.AddFont("Segoe UI Semi Light.ttf", "SegoeSemiLight");
                    fonts.AddFont("Segoe UI.ttf", "Segoe");
                    fonts.AddFont("Segoe UI Bold.ttf", "SegoeBold");

                    // FontAwesome
                    fonts.AddFont("Font Awesome 6 Free-Regular-400.otf", "FARegular");
                    fonts.AddFont("Font Awesome 6 Free-Solid-900.otf", "FASolid");
                })
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddCustomHandlers();
                    handlers.AddPlainer();
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddJsInteropExtensions();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            MauiExceptions.UnhandledException += OnUnhandledException;

            builder.RegisterServices();
            builder.RegisterViewModels();
            builder.RegisterViews();
            builder.ConfigureLogger();

            return builder.Build();
        }

        private static void RegisterViewModels(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<AppShellViewModel>();
            builder.Services.AddSingleton<ConversationListViewModel>();
            builder.Services.AddSingleton<ModelListViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();

            builder.Services.AddSingleton<ChatPageViewModel>();
            builder.Services.AddTransient<ModelsPageViewModel>();
            builder.Services.AddSingleton<AssistantsPageViewModel>();
        }

        private static void RegisterViews(this MauiAppBuilder builder)
        {
            builder.Services.AddTransient<ChatPage>();
            builder.Services.AddTransient<ModelsPage>();
            builder.Services.AddTransient<AssistantsPage>();
        }

        private static void RegisterServices(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<IPopupNavigation>(MopupService.Instance);

            builder.Services.AddSingleton<AppSettingsService>();
            builder.Services.AddSingleton<ILMKitMaestroDatabase, LMKitMaestroDatabase>();
            builder.Services.AddSingleton<ILLMFileManager, LLMFileManager>();
            builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();
            builder.Services.AddSingleton<LMKitMaestro.Services.IPopupService, LMKitMaestro.Services.PopupService>();
            builder.Services.AddSingleton<LMKitMaestro.Services.ILauncher, LMKitMaestro.Services.Launcher>();
            builder.Services.AddSingleton<LMKitMaestro.Services.INavigationService, LMKitMaestro.Services.NavigationService>();
            builder.Services.AddSingleton<LMKitMaestro.Services.IMainThread, LMKitMaestro.Services.MainThread>();
            builder.Services.AddSingleton<CommunityToolkit.Maui.Core.IPopupService, CommunityToolkit.Maui.PopupService>();

#if WINDOWS
            builder.Services.AddSingleton<LMKitMaestro.Services.IFolderPicker, LMKitMaestro.WinUI.FolderPicker>();
#endif

            builder.Services.AddSingleton<IPreferences>(Preferences.Default);
            builder.Services.AddSingleton<LMKitService>();
            builder.Services.AddSingleton<LLMFileManager>();
            builder.Services.AddSingleton<HttpClient>();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                //_logger.LogError("Unhandled exception occurred: " + exception.Message);
            }
        }

        public static void ConfigureLogger(this MauiAppBuilder builder)
        {
            builder.Logging
            .AddTraceLogger(
                options =>
                {
                    options.MinLevel = LogLevel.Trace;
                    options.MaxLevel = LogLevel.Critical;
                }) // Will write to the Debug Output
            .AddInMemoryLogger(
                options =>
                {
                    options.MaxLines = 1024;
                    options.MinLevel = LogLevel.Debug;
                    options.MaxLevel = LogLevel.Critical;
                })
            .AddStreamingFileLogger(
                options =>
                {
                    options.RetainDays = 2;
                    options.FolderPath = Path.Combine(
                        FileSystem.CacheDirectory,
                        "MetroLogs");
                });

            var path = FileSystem.CacheDirectory;
            builder.Services.AddSingleton(LogOperatorRetriever.Instance);
        }
    }
}
