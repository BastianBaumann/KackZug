using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Import;

namespace KackZug.ViewModels;

/// <summary>Eine Teilstrecke im Formular fuer die Eingabe von Hand</summary>
public partial class ManualLegViewModel : ViewModelBase
{
    private readonly Action<ManualLegViewModel> _remove;

    public ManualLegViewModel(Func<IStationSearch> search, Action<ManualLegViewModel> remove)
    {
        _remove = remove;
        From = new StationPickerViewModel("Von", search);
        To = new StationPickerViewModel("Nach", search);
    }

    public StationPickerViewModel From { get; }

    public StationPickerViewModel To { get; }

    [ObservableProperty]
    public partial string Title { get; set; } = "";

    [ObservableProperty]
    public partial bool CanRemove { get; set; }

    [ObservableProperty]
    public partial string DepartureText { get; set; } = "";

    [ObservableProperty]
    public partial string ArrivalText { get; set; } = "";

    [ObservableProperty]
    public partial string LineText { get; set; } = "";

    [ObservableProperty]
    public partial string TrainNumberText { get; set; } = "";

    public ManualLegInput ToInput()
    {
        return new ManualLegInput
        {
            From = From.Selected,
            To = To.Selected,
            DepartureText = DepartureText,
            ArrivalText = ArrivalText,
            LineText = LineText,
            TrainNumberText = TrainNumberText
        };
    }

    [RelayCommand]
    private void Remove()
    {
        _remove(this);
    }
}
