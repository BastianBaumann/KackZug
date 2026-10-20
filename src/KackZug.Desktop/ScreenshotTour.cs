#if DEBUG
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;

namespace KackZug.Desktop;

/// <summary>Geht alle Seiten durch und speichert Bildschirmfotos, gesteuert ueber KACKZUG_SCREENSHOTS</summary>
internal static class ScreenshotTour
{
    private static readonly List<string> Results = new List<string>();

    public static void Start(string directory)
    {
        DispatcherTimer.RunOnce(() => _ = RunAsync(directory), TimeSpan.FromSeconds(2));
    }

    private static async Task RunAsync(string directory)
    {
        Directory.CreateDirectory(directory);
        var lifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        try
        {
            var window = lifetime.MainWindow!;
            window.Width = 400;
            window.Height = 820;
            var main = App.Main!;

            foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                Application.Current.RequestedThemeVariant = variant;
                foreach (var page in new[] { "week", "shifts", "settings" })
                {
                    main.Navigate(page);
                    var name = page + "-" + variant.Key.ToString()!.ToLowerInvariant() + ".png";
                    await CaptureAsync(window, Path.Combine(directory, name));
                    Check(main.CurrentPage is not null, "Seite " + page + " geoeffnet");
                }
            }

            await TourSteps.RunAsync(main, window, directory, Check, CaptureAsync);
        }
        catch (Exception exception)
        {
            File.WriteAllText(Path.Combine(directory, "error.txt"), exception.ToString());
        }

        File.WriteAllLines(Path.Combine(directory, "checks.txt"), Results);
        lifetime.Shutdown();
    }

    private static void Check(bool condition, string description)
    {
        var prefix = "FAIL ";
        if (condition)
        {
            prefix = "OK   ";
        }

        Results.Add(prefix + description);
    }

    private static async Task CaptureAsync(TopLevel target, string path)
    {
        await Task.Delay(700);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        var size = target.ClientSize;
        using var bitmap = new RenderTargetBitmap(new PixelSize((int)size.Width, (int)size.Height), new Vector(96, 96));
        bitmap.Render(target);
        bitmap.Save(path);
    }
}
#endif
