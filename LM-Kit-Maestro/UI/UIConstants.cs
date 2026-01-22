using MudBlazor;

namespace LMKit.Maestro.UI;

public static class UIConstants
{
    public const int ChatWindowLayoutMinimumWidth = 720;
    public const double WindowMinimumWidth = 568;
    public const double WindowMinimumHeight = 600;
    public const int ChatPageSidebarWidth = 300;
    public const int ChatSidebarMinimumWidth = 178;
    public const float ChatSidebarMaxWidthPercents = 0.3f;
    public const float ChatSidebarInitialRatio = 0.2f;

    public static class Colors
    {
        // Unified Emerald theme (matches CSS and ThemeService default)
        public const string Primary = "#10A37F";
        public const string PrimaryAccent = "#0D8A6F";
        public const string Error = "#EF4444";
        public const string Surface = "#171717";
        public const string Background = "#212121";
        public const string OnSurface = "#ECECEC";
        public const string Outline = "#8E8E8E";
        public const string OutlineVariant = "#3A3A3A";
        public const string Secondary = "#10A37F";
    }

    public static readonly MudTheme MaestroTheme = new MudTheme()
    {
        PaletteDark = new()
        {
            Primary = Colors.Primary,
            Secondary = Colors.Secondary,
            Surface = Colors.Surface,
            Background = Colors.Background,
            TextPrimary = Colors.OnSurface,
            TextSecondary = Colors.Outline,
            Divider = Colors.OutlineVariant,
            BackgroundGray = Colors.OutlineVariant,
            Error = Colors.Error,
        },

        Typography = new Typography()
        {
            Overline = new OverlineTypography()
            {
                LineHeight = "1.5",
            }
        }
    };
}
