using MudBlazor;

namespace LMKit.Maestro.Services;

public class ThemeService
{
    public event Action? OnThemeChanged;
    
    private ThemePreset _currentTheme = ThemePresets.Emerald;
    
    public ThemePreset CurrentTheme => _currentTheme;
    
    public MudTheme MudTheme => CreateMudTheme(_currentTheme);
    
    public void SetTheme(ThemePreset theme)
    {
        _currentTheme = theme;
        OnThemeChanged?.Invoke();
    }
    
    public void SetTheme(string themeName)
    {
        var theme = ThemePresets.All.FirstOrDefault(t => t.Name == themeName);
        if (theme != null)
        {
            SetTheme(theme);
        }
    }
    
    private static MudTheme CreateMudTheme(ThemePreset theme)
    {
        return new MudTheme()
        {
            PaletteDark = new PaletteDark()
            {
                Primary = theme.Primary,
                Secondary = theme.Primary,
                Surface = "#171717",
                Background = "#212121",
                TextPrimary = "#ECECEC",
                TextSecondary = "#8E8E8E",
                Divider = "#3A3A3A",
                BackgroundGray = "#3A3A3A",
                Error = "#EF4444",
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
}

public record ThemePreset(string Name, string Primary, string Accent);

public static class ThemePresets
{
    public static readonly ThemePreset Emerald = new("Emerald", "#10A37F", "#0D8A6F");
    public static readonly ThemePreset Ocean = new("Ocean", "#0EA5E9", "#0284C7");
    public static readonly ThemePreset Violet = new("Violet", "#8B5CF6", "#7C3AED");
    public static readonly ThemePreset Rose = new("Rose", "#F43F5E", "#E11D48");
    public static readonly ThemePreset Amber = new("Amber", "#F59E0B", "#D97706");
    public static readonly ThemePreset Cyan = new("Cyan", "#06B6D4", "#0891B2");
    public static readonly ThemePreset Pink = new("Pink", "#EC4899", "#DB2777");
    public static readonly ThemePreset Indigo = new("Indigo", "#6366F1", "#4F46E5");
    
    public static readonly ThemePreset[] All = new[]
    {
        Emerald, Ocean, Violet, Rose, Amber, Cyan, Pink, Indigo
    };
}
