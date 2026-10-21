using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using KackZug.Core.Checking;
using KackZug.Core.Models;
using KackZug.Services;

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
        WeekdayShort = German.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek);
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

    public string WeekdayShort { get; }

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
    public partial bool IsMarked { get; set; }

    [ObservableProperty]
    public partial bool IsSelecting { get; set; }

    public bool CanMark
    {
        get { return IsSelecting && IsEditable; }
    }

    public void ApplyShift(Shift? shift)
    {
        _plan.ShiftId = null;
        if (shift is not null)
        {
            _plan.ShiftId = shift.Id;
        }

        _plan.OutboundEnabled = true;
        _plan.ReturnEnabled = true;
    }

    [ObservableProperty]
    public partial bool OutboundEnabled { get; set; }

    [ObservableProperty]
    public partial bool ReturnEnabled { get; set; }

    [ObservableProperty]
    public partial StatusBadge? OutboundStatus { get; set; }

    [ObservableProperty]
    public partial StatusBadge? ReturnStatus { get; set; }

    [ObservableProperty]
    public partial StatusBadge? DayBadge { get; set; }

    public string? OutboundCaption
    {
        get { return OutboundStatus?.Caption; }
    }

    public string? ReturnCaption
    {
        get { return ReturnStatus?.Caption; }
    }

    public string OutboundLabel
    {
        get { return TripLabel(TripDirection.Outbound); }
    }

    public string ReturnLabel
    {
        get { return TripLabel(TripDirection.Return); }
    }

    public IBrush StripeBrush
    {
        get
        {
            if (Shift is null)
            {
                return ThemeService.FindBrush("AppBorderBrush", Brushes.LightGray);
            }

            return ColorOption.BrushFor(Shift.ColorHex);
        }
    }

    public IBrush ShiftBrush
    {
        get
        {
            if (Shift is null)
            {
                return ThemeService.FindBrush("AppSurfaceAltBrush", Brushes.LightGray);
            }

            return ColorOption.BrushFor(Shift.ColorHex);
        }
    }

    public IBrush ShiftForeground
    {
        get
        {
            if (Shift is null)
            {
                return ThemeService.FindBrush("AppMutedBrush", Brushes.Gray);
            }

            return ThemeService.FindBrush("AppOnShiftBrush", Brushes.White);
        }
    }

    public void Detach()
    {
        _isDetached = true;
    }

    public void RefreshStatus()
    {
        OutboundStatus = BadgeFor(TripDirection.Outbound, HasOutbound && OutboundEnabled);
        ReturnStatus = BadgeFor(TripDirection.Return, HasReturn && ReturnEnabled);
        DayBadge = WorstBadge();
        OnPropertyChanged(nameof(OutboundCaption));
        OnPropertyChanged(nameof(ReturnCaption));
        OnPropertyChanged(nameof(OutboundLabel));
        OnPropertyChanged(nameof(ReturnLabel));
        OnPropertyChanged(nameof(StripeBrush));
        OnPropertyChanged(nameof(ShiftBrush));
        OnPropertyChanged(nameof(ShiftForeground));
    }

    partial void OnIsSelectingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanMark));
        if (!value)
        {
            IsMarked = false;
        }
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

    private string TripLabel(TripDirection direction)
    {
        var prefix = "Hin";
        if (direction == TripDirection.Return)
        {
            prefix = "Rück";
        }

        var connection = Shift?.GetConnection(direction);
        if (connection is null || connection.FirstRide is null)
        {
            return prefix;
        }

        return prefix + " " + connection.FirstRide.PlannedDeparture.ToString("HH:mm");
    }

    private CheckState? StateFor(TripDirection direction)
    {
        if (Shift is null)
        {
            return null;
        }

        return _week.Main.Services.Data.CheckStates.FirstOrDefault(item =>
            item.Date == Date && item.ShiftId == Shift.Id && item.Direction == direction);
    }

    private StatusBadge? BadgeFor(TripDirection direction, bool active)
    {
        if (!active || Shift is null)
        {
            return null;
        }

        var label = TripLabel(direction);
        var state = StateFor(direction);
        if (state is null)
        {
            return new StatusBadge(null, "—", label + " · " + StatusBadge.ShortText(null));
        }

        var badgeText = state.LastBadgeText;
        if (string.IsNullOrEmpty(badgeText))
        {
            badgeText = StatusBadge.ShortText(state.LastStatus);
        }

        var caption = label + " · " + badgeText + " · geprüft " + CheckedText(state.LastCheckedAt);
        return new StatusBadge(state.LastStatus, badgeText, caption);
    }

    private StatusBadge? WorstBadge()
    {
        StatusBadge? worst = null;
        var worstRank = -1;
        foreach (var direction in new[] { TripDirection.Outbound, TripDirection.Return })
        {
            var badge = ReturnStatus;
            if (direction == TripDirection.Outbound)
            {
                badge = OutboundStatus;
            }
            if (badge is null)
            {
                continue;
            }

            var state = StateFor(direction);
            var rank = 0;
            if (state is not null && state.LastStatus is not null)
            {
                rank = (int)state.LastStatus.Value + 1;
            }

            if (rank > worstRank)
            {
                worstRank = rank;
                worst = badge;
            }
        }

        return worst;
    }

    private string CheckedText(DateTimeOffset checkedAt)
    {
        var wallClock = Core.Planning.BerlinTime.ToWallClock(checkedAt, Core.Planning.BerlinTime.Zone);
        var checkedDay = DateOnly.FromDateTime(wallClock);
        var today = DateOnly.FromDateTime(Core.Planning.BerlinTime.ToWallClock(_week.Main.Services.Clock.Now, Core.Planning.BerlinTime.Zone));
        if (checkedDay == today)
        {
            return wallClock.ToString("HH:mm");
        }

        return German.DateTimeFormat.GetAbbreviatedDayName(wallClock.DayOfWeek) + " " + wallClock.ToString("HH:mm");
    }
}
