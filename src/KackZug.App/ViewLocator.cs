using Avalonia.Controls;
using Avalonia.Controls.Templates;
using KackZug.Localization;
using KackZug.ViewModels;
using KackZug.Views;

namespace KackZug;

/// <summary>Ordnet jedem ViewModel fest seine View zu; Seiten werden je Hauptansicht einmal gebaut und wiederverwendet</summary>
public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<PageViewModel, Control> _pageViews = new Dictionary<PageViewModel, Control>();

    public Control? Build(object? param)
    {
        if (param is PageViewModel page)
        {
            if (!_pageViews.TryGetValue(page, out var cached))
            {
                cached = Create(page);
                _pageViews[page] = cached;
            }

            return cached;
        }

        return Create(param);
    }

    private static Control Create(object? param)
    {
        switch (param)
        {
            case WeekViewModel:
                return new WeekView();
            case TripsViewModel:
                return new TripsView();
            case ShiftsViewModel:
                return new ShiftsView();
            case SettingsViewModel:
                return new SettingsView();
            case ImportViewModel:
                return new ImportView();
            case ManualEntryViewModel:
                return new ManualEntryView();
            case TutorialViewModel:
                return new TutorialView();
            default:
                return new TextBlock { Text = UiLanguage.Format(UiStrings.View_Missing, param?.GetType().Name) };
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
