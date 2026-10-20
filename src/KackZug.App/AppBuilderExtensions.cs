using Avalonia;
using Avalonia.Media;

namespace KackZug;

/// <summary>Gemeinsame Einstellungen fuer alle Starts: Schrift Inter wie im Designsystem</summary>
public static class AppBuilderExtensions
{
    public static AppBuilder WithAppFont(this AppBuilder builder)
    {
        return builder
            .WithInterFont()
            .With(new FontManagerOptions { DefaultFamilyName = "fonts:Inter#Inter" });
    }
}
