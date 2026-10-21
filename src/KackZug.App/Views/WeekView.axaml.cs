using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using KackZug.ViewModels;

namespace KackZug.Views;

/// <summary>Ansicht des Wochenplans; langes Druecken auf einen Tag startet die Mehrfachauswahl</summary>
public partial class WeekView : UserControl
{
    public WeekView()
    {
        InitializeComponent();
        AddHandler(InputElement.HoldingEvent, OnHolding);
    }

    private void OnHolding(object? sender, HoldingRoutedEventArgs e)
    {
        if (e.HoldingState != HoldingState.Started || DataContext is not WeekViewModel week || week.IsSelecting)
        {
            return;
        }

        var source = e.Source as Visual;
        var row = source?.FindAncestorOfType<Border>(true);
        while (row is not null && row.DataContext is not DayRowViewModel)
        {
            row = row.FindAncestorOfType<Border>();
        }

        if (row?.DataContext is DayRowViewModel day)
        {
            week.StartSelecting(day);
            e.Handled = true;
        }
    }
}
