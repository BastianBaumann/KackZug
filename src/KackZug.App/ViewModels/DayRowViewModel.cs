using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using KackZug.Core.Models;

namespace KackZug.ViewModels;

/// <summary>Ein Tag im Wochenplan mit Schicht, Fahrten und letztem Stand</summary>
public partial class DayRowViewModel : ViewModelBase
{
    private static readonly CultureInfo German = new CultureInfo("de-DE");

    private readonly WeekViewModel _week;
    private readonly DayPlan _plan;
    private bool _isLoading;
    private bool _isDetached;

    public DayRowViewModel(WeekViewModel week, DayPlan plan, DateOnly date, DateOnly today, ObservableCollection<ShiftOption> options)
    {
        _week = week;
        _plan = plan;
        Date = date;
        Options = options;
        DayName = German.DateTimeFormat.GetDayName(date.DayOfWeek);
        DateText = date.ToString("dd.MM.", German);
        IsPast = date < today;
        IsToday = date == today;

        _isLoading = true;
        SelectedOption = options.FirstOrDefault(option => option.Shift is not null && option.Shift.Id == plan.ShiftId) ?? options[0];
        OutboundEnabled = plan.OutboundEnabled;
        ReturnEnabled = plan.ReturnEnabled;
        _isLoading = false;
        RefreshStatus();
    }

    public DateOnly Date { get; }

    public string DayName { get; }

    public string DateText { get; }

    public bool IsPast { get; }

    public bool IsToday { get; }

    public bool IsEditable
    {
        get { return !IsPast; }
    }

    public ObservableCollection<ShiftOption> Options { get; }

    public Shift? Shift
    {
        get
        {
            if (SelectedOption is null)
            {
                return null;
            }

            return SelectedOption.Shift;
        }
    }

    public bool HasShift
    {
        get { return Shift is not null; }
    }

    public bool HasOutbound
    {
        get { return Shift is not null && Shift.Outbound is not null; }
    }

    public bool HasReturn
    {
        get { return Shift is not null && Shift.Return is not null; }
    }

    public bool CanToggleOutbound
    {
        get { return IsEditable && HasOutbound; }
    }

    public bool CanToggleReturn
    {
        get { return IsEditable && HasReturn; }
    }

    [ObservableProperty]
    public partial ShiftOption? SelectedOption { get; set; }

    [ObservableProperty]
    public partial bool OutboundEnabled { get; set; }

    [ObservableProperty]
    public partial bool ReturnEnabled { get; set; }

    [ObservableProperty]
    public partial StatusBadge? OutboundStatus { get; set; }

    [ObservableProperty]
    public partial StatusBadge? ReturnStatus { get; set; }

    public void Detach()
    {
        _isDetached = true;
    }

    public void RefreshStatus()
    {
        OutboundStatus = BadgeFor(TripDirection.Outbound, HasOutbound && OutboundEnabled);
        ReturnStatus = BadgeFor(TripDirection.Return, HasReturn && ReturnEnabled);
    }

    partial void OnSelectedOptionChanged(ShiftOption? value)
    {
        OnPropertyChanged(nameof(Shift));
        OnPropertyChanged(nameof(HasShift));
        OnPropertyChanged(nameof(HasOutbound));
        OnPropertyChanged(nameof(HasReturn));
        OnPropertyChanged(nameof(CanToggleOutbound));
        OnPropertyChanged(nameof(CanToggleReturn));
        if (_isLoading || _isDetached || value is null)
        {
            return;
        }

        _plan.ShiftId = null;
        if (value.Shift is not null)
        {
            _plan.ShiftId = value.Shift.Id;
        }

        RefreshStatus();
        _week.SaveChanges();
    }

    partial void OnOutboundEnabledChanged(bool value)
    {
        if (_isLoading || _isDetached)
        {
            return;
        }

        _plan.OutboundEnabled = value;
        RefreshStatus();
        _week.SaveChanges();
    }

    partial void OnReturnEnabledChanged(bool value)
    {
        if (_isLoading || _isDetached)
        {
            return;
        }

        _plan.ReturnEnabled = value;
        RefreshStatus();
        _week.SaveChanges();
    }

    private StatusBadge? BadgeFor(TripDirection direction, bool active)
    {
        if (!active || Shift is null)
        {
            return null;
        }

        var connection = Shift.GetConnection(direction);
        var departure = "";
        if (connection is not null && connection.FirstRide is not null)
        {
            departure = connection.FirstRide.PlannedDeparture.ToString("HH:mm") + " ";
        }

        var prefix = "Hin ";
        if (direction == TripDirection.Return)
        {
            prefix = "Rück ";
        }

        var state = _week.Main.Services.Data.CheckStates.FirstOrDefault(item =>
            item.Date == Date && item.ShiftId == Shift.Id && item.Direction == direction);
        if (state is null)
        {
            return new StatusBadge(null, prefix + departure + "· " + StatusBadge.ShortText(null));
        }

        var text = prefix + departure + "· " + StatusBadge.ShortText(state.LastStatus);
        return new StatusBadge(state.LastStatus, text);
    }
}
