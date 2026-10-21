using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using KackZug.ViewModels;

namespace KackZug.Views;

/// <summary>Hauptansicht, die auf Desktop und Handy gleich ist; die Zurueck-Taste schliesst zuerst Detail, Editor oder Dialog</summary>
public partial class MainView : UserControl
{
    private TopLevel? _topLevel;

    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _topLevel = TopLevel.GetTopLevel(this);
        if (_topLevel is not null)
        {
            _topLevel.BackRequested += OnBackRequested;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_topLevel is not null)
        {
            _topLevel.BackRequested -= OnBackRequested;
            _topLevel = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void OnBackRequested(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel main && main.HandleBack())
        {
            e.Handled = true;
        }
    }
}
