using Avalonia.Controls;
using Avalonia.Controls.Templates;
using KackZug.ViewModels;
using KackZug.Views;

namespace KackZug;

/// <summary>Ordnet jedem ViewModel fest seine View zu</summary>
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        switch (param)
        {
            case WeekViewModel:
                return new WeekView();
            case ShiftsViewModel:
                return new ShiftsView();
            case SettingsViewModel:
                return new SettingsView();
            case ImportViewModel:
                return new ImportView();
            case TutorialViewModel:
                return new TutorialView();
            default:
                return new TextBlock { Text = "Keine Ansicht fuer " + param?.GetType().Name };
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
