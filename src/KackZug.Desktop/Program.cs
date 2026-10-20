using Avalonia;

namespace KackZug.Desktop;

/// <summary>Start auf dem Desktop, nur zum schnellen Entwickeln der Oberflaeche</summary>
internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var builder = BuildAvaloniaApp();
        return builder.StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}
