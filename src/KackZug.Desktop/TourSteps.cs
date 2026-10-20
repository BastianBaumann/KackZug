#if DEBUG
using Avalonia.Controls;
using KackZug.ViewModels;

namespace KackZug.Desktop;

/// <summary>Zusaetzliche Ablaeufe der Screenshot-Tour, wird mit jeder Ansicht erweitert</summary>
internal static class TourSteps
{
    public static Task RunAsync(MainViewModel main, Window window, string directory, Action<bool, string> check, Func<TopLevel, string, Task> capture)
    {
        return Task.CompletedTask;
    }
}
#endif
