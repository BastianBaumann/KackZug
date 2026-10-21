using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Checking;
using KackZug.Core.Models;
using KackZug.Core.Planning;
using KackZug.Localization;

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
        get { return UiStrings.Nav_Week; }
    }

    public ObservableCollection<DayRowViewModel> Days { get; } = new ObservableCollection<DayRowViewModel>();

    [ObservableProperty]
    public partial string WeekText { get; set; } = "";

    [ObservableProperty]
    public partial string WeekTitle { get; set; } = UiStrings.Week_ThisWeek;

    [ObservableProperty]
    public partial bool IsThisWeek { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSelecting { get; set; }

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
    private void OpenDetail(DayRowViewModel day)
    {
        if (day.Shift is null)
        {
            return;
        }

        var detail = new TripDetailViewModel(Main, day.Date, day.Shift, day.Plan, CloseDetail);
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

    public bool HandleBack()
    {
        if (Detail is not null)
        {
            CloseDetail();
            return true;
        }

        if (IsSelecting)
        {
            CancelSelectingCommand.Execute(null);
            return true;
        }

        return false;
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
            Main.ShowToast(UiStrings.Week_MarkDaysFirst, false);
            return;
        }

        foreach (var day in marked)
        {
            day.ApplyShift(BulkOption.Shift);
        }

        IsSelecting = false;
        Main.Services.SaveAndReschedule();
        Refresh();
        var dayText = UiLanguage.Format(UiStrings.Week_ManyDays, marked.Count);
        if (marked.Count == 1)
        {
            dayText = UiStrings.Week_OneDay;
        }

        Main.ShowToast(UiLanguage.Format(UiStrings.Week_DaysSet, dayText, BulkOption.Name), false);
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
        try
        {
            var services = Main.Services;
            var today = Today();
            var checker = new TripChecker(services.CreateStatusSource(), services.Clock);
            var checkedTrips = 0;
            foreach (var date in new[] { today, today.AddDays(1) })
            {
                foreach (var trip in TripCalendar.TripsOn(services.Data, date))
                {
                    var outcome = await checker.CheckAsync(services.Data, date, trip.Id, trip.Direction, CheckContext.Manual, CancellationToken.None);
                    if (outcome is not null)
                    {
                        checkedTrips++;
                    }
                }
            }

            services.Store.Save(services.Data);
            Refresh();
            if (checkedTrips == 0)
            {
                Main.ShowToast(UiStrings.Week_NoTrips, false);
            }
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
            WeekTitle = UiStrings.Week_ThisWeek;
            if (IsNextWeek)
            {
                week = data.EnsureNextWeek();
                WeekTitle = UiStrings.Week_NextWeek;
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

            WeekText = UiLanguage.Format(UiStrings.Week_Range, week.WeekStart, week.WeekStart.AddDays(6));
            HasNoShifts = data.Shifts.Count == 0;
            _builtForDay = today;
        }
        finally
        {
            _isRefreshing = false;
        }
    }
}
