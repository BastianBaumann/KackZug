using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KackZug.Services;
using KackZug.ViewModels;
using KackZug.Views;

namespace KackZug;

/// <summary>Start der App, verbindet die Oberflaeche mit der jeweiligen Plattform</summary>
public partial class App : Application
{
    public static IPlatform Platform { get; set; } = new DefaultPlatform();

    public static AppServices? Services { get; private set; }

    public static MainViewModel? Main { get; private set; }

    private static string? _pendingSharedText;

    public static void HandleSharedText(string text)
    {
        var main = Main;
        if (main is null)
        {
            _pendingSharedText = text;
            return;
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(() => _ = main.StartImportAsync(text));
    }

    public static void ReloadFromBackground()
    {
        var services = Services;
        if (services is null)
        {
            return;
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(() => services.Reload());
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = AppServices.Create(Platform);
        Services = services;
        services.Theme.Apply(services.Data.Settings);
        services.Reschedule();

        var main = new MainViewModel(services);
        Main = main;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = main };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime activity)
        {
            activity.MainViewFactory = () => new MainView { DataContext = main };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainView { DataContext = main };
        }

        base.OnFrameworkInitializationCompleted();

        if (_pendingSharedText is not null)
        {
            var text = _pendingSharedText;
            _pendingSharedText = null;
            HandleSharedText(text);
        }
    }
}
