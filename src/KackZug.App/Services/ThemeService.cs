using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using KackZug.Core.Models;

namespace KackZug.Services;

/// <summary>Setzt die zwei Farben und Hell oder Dunkel live</summary>
public sealed class ThemeService
{
    public const string DefaultAccent = "#1E9E8B";
    public const string DefaultSecondary = "#8B5A2B";

    public static readonly (string Name, string Accent, string Secondary)[] Presets =
    {
        ("KackZug", DefaultAccent, DefaultSecondary),
        ("Nachtzug", "#3F51B5", "#7E57C2"),
        ("Regio", "#2E7D32", "#F9A825"),
        ("Ostsee", "#0277BD", "#26A69A"),
        ("Anthrazit", "#37474F", "#FB8C00")
    };

    public void Apply(Settings settings)
    {
        var application = Application.Current;
        if (application is null)
        {
            return;
        }

        application.RequestedThemeVariant = ToThemeVariant(settings.ThemeMode);

        var accent = Parse(settings.AccentColor, DefaultAccent);
        var secondary = Parse(settings.SecondaryColor, DefaultSecondary);
        var resources = application.Resources;

        resources["AppAccentColor"] = accent;
        resources["AppSecondaryColor"] = secondary;
        resources["AppAccentBrush"] = new SolidColorBrush(accent);
        resources["AppSecondaryBrush"] = new SolidColorBrush(secondary);
        resources["AppAccentSoftBrush"] = new SolidColorBrush(Color.FromArgb(0x26, accent.R, accent.G, accent.B));
        resources["AppGradientBrush"] = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops = { new GradientStop(accent, 0), new GradientStop(secondary, 1) }
        };

        resources["SystemAccentColor"] = accent;
        resources["SystemAccentColorLight1"] = Shade(accent, 1.15);
        resources["SystemAccentColorLight2"] = Shade(accent, 1.30);
        resources["SystemAccentColorLight3"] = Shade(accent, 1.45);
        resources["SystemAccentColorDark1"] = Shade(accent, 0.85);
        resources["SystemAccentColorDark2"] = Shade(accent, 0.70);
        resources["SystemAccentColorDark3"] = Shade(accent, 0.55);
    }

    public static Color Parse(string? hex, string fallback)
    {
        if (Color.TryParse(hex, out var color))
        {
            return color;
        }

        return Color.Parse(fallback);
    }

    public static string ToHex(Color color)
    {
        return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
    }

    private static ThemeVariant ToThemeVariant(ThemeMode mode)
    {
        switch (mode)
        {
            case ThemeMode.Light:
                return ThemeVariant.Light;
            case ThemeMode.Dark:
                return ThemeVariant.Dark;
            default:
                return ThemeVariant.Default;
        }
    }

    private static Color Shade(Color color, double factor)
    {
        return Color.FromArgb(color.A, ClampToByte(color.R * factor), ClampToByte(color.G * factor), ClampToByte(color.B * factor));
    }

    private static byte ClampToByte(double value)
    {
        return (byte)Math.Clamp(value, 0, 255);
    }
}
