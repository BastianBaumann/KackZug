using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KackZug.ViewModels;

/// <summary>Eine waehlbare Farbe fuer Schichten</summary>
public partial class ColorOption : ObservableObject
{
    public static readonly string[] ShiftColors =
    {
        "#1E9E8B", "#8B5A2B", "#3F51B5", "#E9A23B", "#C2185B", "#2E7D32", "#0277BD", "#607D8B"
    };

    public ColorOption(string hex)
    {
        Hex = hex;
        Brush = new SolidColorBrush(Color.Parse(hex));
    }

    public string Hex { get; }

    public IBrush Brush { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
