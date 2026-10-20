using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using KackZug.Services;

namespace KackZug.ViewModels;

/// <summary>Eine der sechs Schichtfarben aus dem Designsystem</summary>
public partial class ColorOption : ObservableObject
{
    public static readonly string[] ShiftColors =
    {
        "#1455C0", "#6A3FB5", "#00788A", "#B8247A", "#8A5523", "#46505E"
    };

    public ColorOption(string hex)
    {
        Hex = hex;
        Brush = BrushFor(hex);
    }

    public string Hex { get; }

    public IBrush Brush { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public static IBrush BrushFor(string? hex)
    {
        var index = Array.FindIndex(ShiftColors, color => string.Equals(color, hex, StringComparison.OrdinalIgnoreCase));
        var fallback = new SolidColorBrush(ThemeService.Parse(hex, ShiftColors[0]));
        if (index < 0)
        {
            return fallback;
        }

        return ThemeService.FindBrush("Shift" + (index + 1) + "Brush", fallback);
    }
}
