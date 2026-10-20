using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Models;

namespace KackZug.ViewModels;

/// <summary>Hin- oder Rueckfahrt einer Schicht im Editor</summary>
public partial class ConnectionCardViewModel : ViewModelBase
{
    private readonly Shift _shift;
    private readonly Action _changed;

    public ConnectionCardViewModel(Shift shift, TripDirection direction, Action changed)
    {
        _shift = shift;
        _changed = changed;
        Direction = direction;
        Title = Core.Checking.AlertComposer.DirectionName(direction);
        Refresh();
    }

    public TripDirection Direction { get; }

    public string Title { get; }

    public ObservableCollection<LegLine> Legs { get; } = new ObservableCollection<LegLine>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    public partial bool HasConnection { get; set; }

    public bool IsEmpty
    {
        get { return !HasConnection; }
    }

    [RelayCommand]
    private void Remove()
    {
        _shift.SetConnection(Direction, null);
        Refresh();
        _changed();
    }

    private void Refresh()
    {
        Legs.Clear();
        var connection = _shift.GetConnection(Direction);
        if (connection is not null)
        {
            foreach (var leg in connection.Legs)
            {
                Legs.Add(new LegLine(leg));
            }
        }

        HasConnection = Legs.Count > 0;
    }
}
