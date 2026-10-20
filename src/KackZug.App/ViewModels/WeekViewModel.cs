using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Checking;
using KackZug.Core.Models;
using KackZug.Core.Planning;

namespace KackZug.ViewModels;

/// <summary>Wochenplan: welcher Tag welche Schicht hat, mit letztem Pruefergebnis</summary>
public partial class WeekViewModel : PageViewModel
{
    private bool _isRefreshing;
    private bool _isSavingOwnChange;
    private DateOnly _builtForDay;

    public WeekViewModel(MainViewModel main)
    {
        Main = main;
        main.Services.DataChanged += (_, _) => OnDataChanged();
        Refresh();
    }

    public MainViewModel Main { get; }

    public override string Title
    {
        get { return "Woche"; }
    }

    public ObservableCollection<DayRowViewModel> Days { get; } = new ObservableCollection<DayRowViewModel>();

    [ObservableProperty]
    public partial string WeekText { get; set; } = "";

    [ObservableProperty]
    public partial bool HasNoShifts { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckNowCommand))]
    public partial bool IsChecking { get; set; }

    [ObservableProperty]
    public partial string? CheckResult { get; set; }

    public override void OnShown()
    {
        if (Today() != _builtForDay)
        {
            Refresh();
        }
    }

    public void SaveChanges()
    {
        if (_isRefreshing)
        {
            return;
        }

        _isSavingOwnChange = true;
        try
        {
            Main.Services.SaveAndReschedule();
        }
        finally
        {
            _isSavingOwnChange = false;
        }
    }

    private void OnDataChanged()
    {
        if (_isSavingOwnChange)
        {
            return;
        }

        Refresh();
    }

    [RelayCommand(CanExecute = nameof(CanCheckNow))]
    private async Task CheckNowAsync()
    {
        IsChecking = true;
        CheckResult = null;
        try
        {
            var services = Main.Services;
            var today = Today();
            var checker = new TripChecker(services.CreateStatusSource(), services.Clock);
            var results = new List<string>();
            foreach (var date in new[] { today, today.AddDays(1) })
            {
                foreach (var trip in TripCalendar.TripsOn(services.Data, date))
                {
                    var outcome = await checker.CheckAsync(services.Data, date, trip.Shift.Id, trip.Direction, CheckContext.Manual, CancellationToken.None);
                    if (outcome is not null)
                    {
                        results.Add(outcome.Alert.Title);
                    }
                }
            }

            services.Store.Save(services.Data);
            if (results.Count == 0)
            {
                CheckResult = "Heute und morgen stehen keine Fahrten an.";
            }
            else
            {
                CheckResult = string.Join("\n", results);
            }

            Refresh();
        }
        finally
        {
            IsChecking = false;
        }
    }

    private bool CanCheckNow()
    {
        return !IsChecking;
    }

    private DateOnly Today()
    {
        return DateOnly.FromDateTime(BerlinTime.ToWallClock(Main.Services.Clock.Now, BerlinTime.Zone));
    }

    private void Refresh()
    {
        _isRefreshing = true;
        try
        {
            var data = Main.Services.Data;
            var today = Today();
            Main.Services.RunMaintenance();
            var week = data.CurrentWeek!;

            foreach (var oldRow in Days)
            {
                oldRow.Detach();
            }

            Days.Clear();
            foreach (var day in WeekPlan.OrderedDays)
            {
                var options = new ObservableCollection<ShiftOption> { new ShiftOption(null) };
                foreach (var shift in data.Shifts)
                {
                    options.Add(new ShiftOption(shift));
                }

                var date = week.DateOf(day);
                Days.Add(new DayRowViewModel(this, week.GetDay(day), date, today, options));
            }

            WeekText = "Woche vom " + week.WeekStart.ToString("dd.MM.") + " bis " + week.WeekStart.AddDays(6).ToString("dd.MM.yy");
            HasNoShifts = data.Shifts.Count == 0;
            _builtForDay = today;
        }
        finally
        {
            _isRefreshing = false;
        }
    }
}
