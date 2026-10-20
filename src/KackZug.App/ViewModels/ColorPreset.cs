using Avalonia.Media;

namespace KackZug.ViewModels;

/// <summary>Vorgefertigtes Farbpaar fuer das Aussehen</summary>
public sealed class ColorPreset
{
    public ColorPreset(string name, string accent, string secondary)
    {
        Name = name;
        Accent = accent;
        Secondary = secondary;
        Brush = new LinearGradientBrush
        {
            StartPoint = new Avalonia.RelativePoint(0, 1, Avalonia.RelativeUnit.Relative),
            EndPoint = new Avalonia.RelativePoint(1, 0, Avalonia.RelativeUnit.Relative),
            GradientStops = { new GradientStop(Color.Parse(accent), 0), new GradientStop(Color.Parse(secondary), 1) }
        };
    }

    public string Name { get; }

    public string Accent { get; }

    public string Secondary { get; }

    public IBrush Brush { get; }
}
