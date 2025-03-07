using MudBlazor;

namespace LMKit.Maestro.UI;

public static class UIConstants
{
    public const double ChatWindowLayoutMinimumWidth = 720;

    public const double WindowMinimumWidth = 568;

    public const double WindowMinimumHeight = 600;

    public const double AlertPopupWidth = 400;

    public const double ChatPageSidebarWidth = 300;

    public const double ChatMessageMaximumWidth = 800;

    public const double TabBarHeight = 48;

    public const double PageTopBarHeight = 64;

    public const double ModelSelectionButtonHeight = 48;

    public const double HeaderHorizontalMargin = 12;

    public const double ChatPageSideTogglesWidth = (ChatPageToggleButtonWidth * 2) + 16 + 8;

    public const double ChatPageToggleButtonWidth = 32;

    public const double ModelSelectionButtonMaxWidth = 568;

    public const int ChatSidebarInitialWidth = 300;

    public const int ChatSidebarMinimumWidth = 180;

    public const float ChatSidebarMaxWidthPercents = 0.3f;

    public const float ChatSidebarInitialRatio = 0.2f;

    public static class Colors
    {
        public const string Primary = "#512BD4";
        public const string PrimaryAccent = "#512BD4";
        public const string Error = "#FF5551";
        public const string Surface = "#06080A";
        public const string Background = "#0C1014";
        public const string OnSurface = "#FFFFFF";
        public const string Outline = "#9198A1";
        public const string OutlineVariant = "#2E3033";
        public const string Secondary = "#096BDE";
    }

    public static readonly MudTheme MaestroTheme = new MudTheme()
    {
        PaletteDark = new()
        {
            Primary = Colors.Primary,
            Surface = Colors.Surface,
            Background = Colors.Background,
            TextPrimary = Colors.OnSurface,
            Divider = Colors.OutlineVariant,
            BackgroundGray = Colors.OutlineVariant,
            Error = Colors.Error,
            Secondary = Colors.Secondary,
            TextSecondary = Colors.Outline,
            //AppbarText = "#92929f",
            //AppbarBackground = "rgba(26,26,39,0.8)",
            //DrawerBackground = "#1a1a27",
            //ActionDefault = "#74718e",
            //ActionDisabled = "#9999994d",
            //ActionDisabledBackground = "#605f6d4d",
            //TextDisabled = "#ffffff33",
            //DrawerIcon = "#92929f",
            //DrawerText = "#92929f",
            //GrayLight = "#2a2833",
            //GrayLighter = "#06080A",
            //Info = "#4a86ff",
            //Success = "#3dcb6c",
            //Warning = "#ffb545",
            //LinesDefault = "#33323e",
            //TableLines = "#33323e",
            //OverlayLight = "#12181F",
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
