using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KackZug.Localization;
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

    private static ContentControl? _activityHost;

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

    /// <summary>Baut die Oberflaeche nach einem Sprachwechsel neu auf und zeigt wieder die Einstellungen</summary>
    public static void ReloadUi()
    {
        var services = Services;
        if (services is null || Current is not App app)
        {
            return;
        }

        var main = new MainViewModel(services);
        Main = main;
        main.Navigate("settings");
        if (_activityHost is not null)
        {
            _activityHost.Content = new MainView { DataContext = main };
        }
        else if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
        {
            desktop.MainWindow.Content = new MainView { DataContext = main };
        }
        else if (app.ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainView { DataContext = main };
        }
        else
        {
            Platform.RecreateMainView();
        }
    }

    /// <remarks>Die Aktivitaet bekommt einen festen Rahmen, damit ein Sprachwechsel nur dessen Inhalt tauscht und die Aktivitaet bleibt.
    /// Auf Android ist die Lebensdauer auch eine ISingleViewApplicationLifetime, deren MainView dort aber nicht angezeigt wird.</remarks>
    private static Control CreateActivityContent()
    {
        var host = new ContentControl { Content = new MainView { DataContext = Main } };
        _activityHost = host;
        return host;
    }

    /// <summary>Beim Zurueckkehren in die App: wartet ein Update auf die Erlaubnis zum Installieren, geht es weiter</summary>
    public static void ContinueUpdateAfterResume()
    {
        var main = Main;
        if (main is null)
        {
            return;
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(() => _ = main.ContinueUpdateAfterPermissionAsync());
    }

    /// <summary>Ergebnis einer Update-Installation, die nicht mit dem Neustart der App endete</summary>
    public static void ReportUpdateInstallFinished(bool succeeded, bool aborted, string? message)
    {
        var main = Main;
        if (main is null)
        {
            return;
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(() => main.ReportInstallFinished(succeeded, aborted, message));
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = AppServices.Create(Platform);
        Services = services;
        UiLanguage.Apply(services.Data.Settings.Language);
        services.Theme.Apply(services.Data.Settings);
        services.Reschedule();

        var main = new MainViewModel(services);
        Main = main;
        main.ShowTutorialIfNeeded();
        services.Updates.RemoveOutdatedDownloads();
        _ = main.CheckForUpdateAsync();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = main };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime activity)
        {
            activity.MainViewFactory = CreateActivityContent;
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
