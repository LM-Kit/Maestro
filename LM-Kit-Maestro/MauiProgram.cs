using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using LMKit.Maestro.Data;
using LMKit.Maestro.Handlers;
using LMKit.Maestro.Services;
using LMKit.Maestro.UI;
using LMKit.Maestro.UI.Pages;
using LMKit.Maestro.ViewModels;
using Majorsoft.Blazor.Components.Common.JsInterop;
using MetroLog.MicrosoftExtensions;
using MetroLog.Operators;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using Mopups.Services;
using MudBlazor.Services;
using SimpleToolkit.SimpleShell;

namespace LMKit.Maestro
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
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

                    fonts.AddFont("MaterialIcons-Regular.ttf", "Material");

                    //fonts.AddFont("FontAkwesome.ttf", "Material");
                    // FontAwesome
                    //fonts.AddFont("Font Awesome 6 Free-Regular-400.otf", "FARegular");
                })
                .ConfigureMauiHandlers(handlers => { handlers.AddCustomHandlers(); });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddJsInteropExtensions();
            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.SnackbarVariant = MudBlazor.Variant.Filled;
            });

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
            builder.Services.AddTransient<MainPage>();
        }

        private static void RegisterServices(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton(MopupService.Instance);

            builder.Services.AddSingleton<AppSettingsService>();
            builder.Services.AddSingleton<IMaestroDatabase, MaestroDatabase>();
            builder.Services.AddSingleton<ILLMFileManager, LLMFileManager>();
            builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();
            builder.Services.AddSingleton<IPopupService, Services.PopupService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<IMainThread, Services.MainThread>();
            builder.Services.AddSingleton<CommunityToolkit.Maui.Core.IPopupService, CommunityToolkit.Maui.PopupService>();
            builder.Services.AddSingleton<ISnackbarService, SnackbarService>();

            builder.Services.AddSingleton<IFolderPicker>(FolderPicker.Default);

            builder.Services.AddSingleton(Launcher.Default);
            builder.Services.AddSingleton(Preferences.Default);
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