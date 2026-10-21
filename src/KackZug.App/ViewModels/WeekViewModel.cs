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
    public partial string WeekTitle { get; set; } = "Diese Woche";

    [ObservableProperty]
    public partial bool IsThisWeek { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSelecting { get; set; }

    [ObservableProperty]
    public partial ShiftOption? BulkOption { get; set; }

    public ObservableCollection<ShiftOption> BulkOptions { get; } = new ObservableCollection<ShiftOption>();

    [ObservableProperty]
    public partial bool IsNextWeek { get; set; }

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

    [RelayCommand]
    public void StartSelecting(DayRowViewModel? firstDay)
    {
        BulkOptions.Clear();
        BulkOptions.Add(new ShiftOption(null));
        foreach (var shift in Main.Services.Data.Shifts)
        {
            BulkOptions.Add(new ShiftOption(shift));
        }

        BulkOption = BulkOptions.FirstOrDefault(option => option.Shift is not null) ?? BulkOptions[0];
        IsSelecting = true;
        foreach (var day in Days)
        {
            day.IsSelecting = true;
        }

        if (firstDay is not null && firstDay.IsEditable)
        {
            firstDay.IsMarked = true;
        }
    }

    [RelayCommand]
    private void CancelSelecting()
    {
        IsSelecting = false;
        foreach (var day in Days)
        {
            day.IsSelecting = false;
        }
    }

    [RelayCommand]
    private void ApplySelection()
    {
        var marked = Days.Where(day => day.IsMarked && day.IsEditable).ToList();
        if (marked.Count == 0 || BulkOption is null)
        {
            Main.ShowToast("Markier erst ein paar Tage.", false);
            return;
        }

        foreach (var day in marked)
        {
            day.ApplyShift(BulkOption.Shift);
        }

        IsSelecting = false;
        Main.Services.SaveAndReschedule();
        Refresh();
        var dayText = marked.Count + " Tage";
        if (marked.Count == 1)
        {
            dayText = "1 Tag";
        }

        Main.ShowToast(dayText + " auf „" + BulkOption.Name + "“ gesetzt.", false);
    }

    partial void OnIsNextWeekChanged(bool value)
    {
        if (_isRefreshing)
        {
            return;
        }

        Refresh();
    }

    private void OnDataChanged()
    {
        if (_isSavingOwnChange || IsSelecting)
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
            WeekTitle = "Diese Woche";
            if (IsNextWeek)
            {
                week = data.EnsureNextWeek();
                WeekTitle = "Nächste Woche";
            }

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
                var row = new DayRowViewModel(this, week.GetDay(day), date, today, options);
                row.IsSelecting = IsSelecting;
                Days.Add(row);
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
