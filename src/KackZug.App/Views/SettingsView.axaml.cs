using Avalonia.Controls;
using Avalonia.Interactivity;
using KackZug.ViewModels;

namespace KackZug.Views;

/// <summary>Ansicht der Einstellungen</summary>
public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void OnEveningTimeLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel settings)
        {
            settings.TidyEveningCheckTime();
        }
    }
}
