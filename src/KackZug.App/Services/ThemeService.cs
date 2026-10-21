using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using KackZug.Core.Models;
using KackZug.Localization;

namespace KackZug.Services;

/// <summary>Setzt Hell oder Dunkel und, falls gewaehlt, eine eigene Markenfarbe statt des Orange aus dem Designsystem</summary>
public sealed class ThemeService
{
    public const string DefaultAccent = "#C84B0C";
    public const string LegacyAccent = "#1E9E8B";

    /// <remarks>Die Namen kommen bei jedem Abruf neu aus der Texttabelle, damit sie der eingestellten Sprache folgen.</remarks>
    public static (string Name, string Accent)[] Presets
    {
        get
        {
            return new[]
            {
                (UiStrings.Settings_PresetOrange, DefaultAccent),
                (UiStrings.Settings_PresetPetrol, "#00788A"),
                (UiStrings.Settings_PresetBlue, "#1455C0"),
                (UiStrings.Settings_PresetViolet, "#6A3FB5"),
                (UiStrings.Settings_PresetSlate, "#46505E")
            };
        }
    }

    private static readonly string[] AccentKeys = { "AppAccentBrush", "AppAccentPressedBrush", "AppAccentSoftBrush", "AppAccentInkBrush" };

    public void Apply(Settings settings)
    {
        var application = Application.Current;
        if (application is null)
        {
            return;
        }

        application.RequestedThemeVariant = ToThemeVariant(settings.ThemeMode);
        var resources = application.Resources;
        var accent = Parse(settings.AccentColor, DefaultAccent);
        var isDefault = IsDefault(settings.AccentColor);

        foreach (var key in AccentKeys)
        {
            resources.Remove(key);
        }

        if (!isDefault)
        {
            resources["AppAccentBrush"] = new SolidColorBrush(accent);
            resources["AppAccentPressedBrush"] = new SolidColorBrush(Shade(accent, 0.83));
            resources["AppAccentInkBrush"] = new SolidColorBrush(accent);
            resources["AppAccentSoftBrush"] = new SolidColorBrush(Color.FromArgb(0x2E, accent.R, accent.G, accent.B));
        }

        resources["SystemAccentColor"] = accent;
        resources["SystemAccentColorLight1"] = Shade(accent, 1.15);
        resources["SystemAccentColorLight2"] = Shade(accent, 1.30);
        resources["SystemAccentColorLight3"] = Shade(accent, 1.45);
        resources["SystemAccentColorDark1"] = Shade(accent, 0.85);
        resources["SystemAccentColorDark2"] = Shade(accent, 0.70);
        resources["SystemAccentColorDark3"] = Shade(accent, 0.55);
    }

    public static bool IsDefault(string? hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return true;
        }

        return string.Equals(hex, DefaultAccent, StringComparison.OrdinalIgnoreCase)
            || string.Equals(hex, LegacyAccent, StringComparison.OrdinalIgnoreCase);
    }

    public static Color Parse(string? hex, string fallback)
    {
        if (IsDefault(hex))
        {
            return Color.Parse(fallback);
        }

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

    public static IBrush FindBrush(string key, IBrush fallback)
    {
        var application = Application.Current;
        if (application is not null && application.TryGetResource(key, application.ActualThemeVariant, out var resource) && resource is IBrush brush)
        {
            return brush;
        }

        return fallback;
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
