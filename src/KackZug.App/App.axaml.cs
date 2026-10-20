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
    }
}
