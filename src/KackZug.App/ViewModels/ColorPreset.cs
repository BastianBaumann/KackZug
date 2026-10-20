using Avalonia.Media;

namespace KackZug.ViewModels;

/// <summary>Vorgefertigte Markenfarbe fuer das Aussehen</summary>
public sealed class ColorPreset
{
    public ColorPreset(string name, string accent)
    {
        Name = name;
        Accent = accent;
        Brush = new SolidColorBrush(Color.Parse(accent));
    }

    public string Name { get; }

    public string Accent { get; }

    public IBrush Brush { get; }
}
