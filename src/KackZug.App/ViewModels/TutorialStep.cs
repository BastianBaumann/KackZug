using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KackZug.ViewModels;

/// <summary>Eine Seite der kurzen Einfuehrung</summary>
public partial class TutorialStep : ObservableObject
{
    public TutorialStep(string title, string text, string? iconKey)
    {
        Title = title;
        Text = text;
        if (iconKey is not null)
        {
            var application = Application.Current;
            if (application is not null && application.TryGetResource(iconKey, null, out var resource))
            {
                Icon = resource as Geometry;
            }
        }
    }

    public string Title { get; }

    public string Text { get; }

    public Geometry? Icon { get; }

    public bool ShowsLogo
    {
        get { return Icon is null; }
    }

    [ObservableProperty]
    public partial bool IsCurrent { get; set; }
}
