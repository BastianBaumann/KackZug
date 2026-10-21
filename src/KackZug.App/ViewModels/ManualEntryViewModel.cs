using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Import;
using KackZug.Core.Models;

namespace KackZug.ViewModels;

/// <summary>Formular fuer eine Verbindung von Hand, wenn ein geteilter Link nicht geht</summary>
public partial class ManualEntryViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private readonly Action<SavedConnection> _done;

    public ManualEntryViewModel(MainViewModel main, Action<SavedConnection> done)
    {
        _main = main;
        _done = done;
        AddLeg();
    }

    public ObservableCollection<ManualLegViewModel> Legs { get; } = new ObservableCollection<ManualLegViewModel>();

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [RelayCommand]
    public void AddLeg()
    {
        var leg = new ManualLegViewModel(() => _main.Services.StationSearch, RemoveLeg);
        if (Legs.Count > 0)
        {
            var previous = Legs[Legs.Count - 1];
            if (previous.To.Selected is not null)
            {
                leg.From.Choose(previous.To.Selected);
            }
        }

        Legs.Add(leg);
        Renumber();
    }

    [RelayCommand]
    public void Continue()
    {
        var inputs = Legs.Select(leg => leg.ToInput()).ToList();
        var result = ManualConnectionBuilder.Build(inputs, _main.Services.Clock.Now);
        ErrorMessage = result.Error;
        if (result.Connection is not null)
        {
            _done(result.Connection);
        }
    }

    private void RemoveLeg(ManualLegViewModel leg)
    {
        if (Legs.Count <= 1)
        {
            return;
        }

        Legs.Remove(leg);
        Renumber();
    }

    private void Renumber()
    {
        for (var index = 0; index < Legs.Count; index++)
        {
            Legs[index].Title = "Teilstrecke " + (index + 1);
            Legs[index].CanRemove = Legs.Count > 1;
        }
    }
}
