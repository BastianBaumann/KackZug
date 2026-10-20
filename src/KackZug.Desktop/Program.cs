using Avalonia;

namespace KackZug.Desktop;

/// <summary>Start auf dem Desktop, nur zum schnellen Entwickeln der Oberflaeche</summary>
internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var builder = BuildAvaloniaApp();

#if DEBUG
        var screenshotDirectory = Environment.GetEnvironmentVariable("KACKZUG_SCREENSHOTS");
        if (!string.IsNullOrEmpty(screenshotDirectory))
        {
            DemoData.WriteIfRequested();
            builder = builder.AfterSetup(_ => ScreenshotTour.Start(screenshotDirectory));
        }
#endif

        return builder.StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithAppFont()
            .LogToTrace();
    }
}
