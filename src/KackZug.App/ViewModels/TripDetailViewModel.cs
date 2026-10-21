using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Checking;
using KackZug.Core.Models;
using KackZug.Core.Planning;

namespace KackZug.ViewModels;

/// <summary>Detailansicht eines Tages: alle Fahrten mit Live-Stand, auf Wunsch neu geprueft</summary>
public partial class TripDetailViewModel : ViewModelBase
{
    private static readonly CultureInfo German = new CultureInfo("de-DE");

    private readonly MainViewModel _main;
    private readonly Action _close;

    public TripDetailViewModel(MainViewModel main, DateOnly date, Shift shift, DayPlan plan, Action close)
    {
        _main = main;
        _close = close;
        Date = date;
        Heading = German.DateTimeFormat.GetDayName(date.DayOfWeek) + ", " + date.ToString("dd.MM.", German);
        ShiftName = shift.Name;

        foreach (var direction in new[] { TripDirection.Outbound, TripDirection.Return })
        {
            var connection = shift.GetConnection(direction);
            if (connection is null || connection.FirstRide is null || !plan.IsTripEnabled(direction))
            {
                continue;
            }

            var state = main.Services.Data.CheckStates.FirstOrDefault(item =>
                item.Date == date && item.ShiftId == shift.Id && item.Direction == direction);
            Trips.Add(new TripCardViewModel(main.Services.Platform, date, shift, direction, connection, state));
        }

        HasNoTrips = Trips.Count == 0;
    }

    public DateOnly Date { get; }

    public string Heading { get; }

    public string ShiftName { get; }

    public ObservableCollection<TripCardViewModel> Trips { get; } = new ObservableCollection<TripCardViewModel>();

    public bool HasNoTrips { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    public partial bool IsChecking { get; set; }

    public bool IsSoon
    {
        get
        {
            var today = DateOnly.FromDateTime(BerlinTime.ToWallClock(_main.Services.Clock.Now, BerlinTime.Zone));
            return Date >= today && Date <= today.AddDays(1);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    public async Task RefreshAsync()
    {
        IsChecking = true;
        try
        {
            var services = _main.Services;
            var checker = new TripChecker(services.CreateStatusSource(), services.Clock);
            foreach (var trip in Trips)
            {
                var outcome = await checker.CheckAsync(services.Data, Date, trip.Shift.Id, trip.Direction, CheckContext.Manual, CancellationToken.None);
                if (outcome is not null)
                {
                    trip.Show(outcome.Status, null);
                }
            }

            services.Store.Save(services.Data);
        }
        finally
        {
            IsChecking = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        _close();
    }

    private bool CanRefresh()
    {
        return !IsChecking;
    }
}
