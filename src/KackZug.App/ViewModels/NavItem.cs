using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KackZug.ViewModels;

/// <summary>Ein Tab in der unteren Leiste</summary>
public partial class NavItem : ObservableObject
{
    public NavItem(string key, string title, string iconKey, Func<PageViewModel> factory)
    {
        Key = key;
        Title = title;
        Icon = FindIcon(iconKey);
        Factory = factory;
    }

    public string Key { get; }

    public string Title { get; }

    public Geometry? Icon { get; }

    public Func<PageViewModel> Factory { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    private static Geometry? FindIcon(string iconKey)
    {
        var application = Application.Current;
        if (application is not null && application.TryGetResource(iconKey, null, out var resource))
        {
            return resource as Geometry;
        }

        return null;
    }
}
