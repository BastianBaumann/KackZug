using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Models;

namespace KackZug.ViewModels;

/// <summary>Ein Treffer der Bahnhofssuche, der sich selbst auswaehlt</summary>
public partial class StationResult : ViewModelBase
{
    private readonly Action<Station> _choose;

    public StationResult(Station station, Action<Station> choose)
    {
        Station = station;
        _choose = choose;
    }

    public Station Station { get; }

    public string Name
    {
        get { return Station.Name; }
    }

    [RelayCommand]
    private void Choose()
    {
        _choose(Station);
    }
}
