using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Planning;
using KackZug.Localization;

namespace KackZug.ViewModels;

/// <summary>Liste der anstehenden Einzelfahrten, nach Abfahrt sortiert</summary>
public partial class TripsViewModel : PageViewModel
{
    public TripsViewModel(MainViewModel main)
    {
        Main = main;
        main.Services.DataChanged += (_, _) => OnDataChanged();
        Refresh();
    }

    public MainViewModel Main { get; }

    public override string Title
    {
        get { return UiStrings.Nav_Trips; }
    }

    public ObservableCollection<OneOffTripCardViewModel> Trips { get; } = new ObservableCollection<OneOffTripCardViewModel>();

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDetail))]
    [NotifyPropertyChangedFor(nameof(IsListVisible))]
    public partial TripDetailViewModel? Detail { get; set; }

    public bool HasDetail
    {
        get { return Detail is not null; }
    }

    public bool IsListVisible
    {
        get { return Detail is null; }
    }

    /// <summary>Beim Zeigen wird aufgeraeumt, damit vorbei gefahrene Fahrten verschwinden</summary>
    public override void OnShown()
    {
        if (Detail is null)
        {
            Refresh();
        }
    }

    public bool HandleBack()
    {
        if (Detail is null)
        {
            return false;
        }

        CloseDetail();
        return true;
    }

    [RelayCommand]
    private void OpenDetail(OneOffTripCardViewModel card)
    {
        var detail = new TripDetailViewModel(Main, card.Trip, CloseDetail);
        Detail = detail;
        if (detail.IsSoon && detail.Trips.Count > 0)
        {
            _ = detail.RefreshAsync();
        }
    }

    public void CloseDetail()
    {
        Detail = null;
        Refresh();
    }

    [RelayCommand]
    private async Task DeleteAsync(OneOffTripCardViewModel card)
    {
        var confirmed = await Main.ConfirmAsync(
            UiStrings.Trips_DeleteTitle,
            UiLanguage.Format(UiStrings.Trips_DeleteMessage, card.Name, card.ShortDateText),
            UiStrings.Shifts_DeleteConfirm);
        if (!confirmed)
        {
            return;
        }

        Main.Services.Data.RemoveTrip(card.Trip.Id);
        Main.Services.SaveAndReschedule();
        Main.ShowToast(UiStrings.Trips_Deleted, false);
        Refresh();
    }

    [RelayCommand]
    private void EnterManually()
    {
        Main.StartManualEntry(true);
    }

    private void OnDataChanged()
    {
        if (Detail is null)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        var services = Main.Services;
        services.RunMaintenance();
        var today = DateOnly.FromDateTime(BerlinTime.ToWallClock(services.Clock.Now, BerlinTime.Zone));
        var upcoming = services.Data.Trips
            .Where(trip => trip.Connection.FirstRide is not null)
            .OrderBy(trip => trip.Connection.FirstRide!.DepartureOn(trip.TravelDate))
            .ToList();

        Trips.Clear();
        foreach (var trip in upcoming)
        {
            Trips.Add(new OneOffTripCardViewModel(Main, trip, today));
        }

        IsEmpty = Trips.Count == 0;
    }
}
