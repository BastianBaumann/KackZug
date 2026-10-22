using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Import;
using KackZug.Core.Localization;
using KackZug.Core.Models;
using KackZug.Core.Planning;
using KackZug.Localization;

namespace KackZug.ViewModels;

/// <summary>Dialog nach dem Teilen: Verbindung laden und einer Schicht als Hin- oder Rueckfahrt zuordnen oder als Einzelfahrt speichern</summary>
public partial class ImportViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private SavedConnection? _connection;
    private DateOnly? _sharedDate;
    private readonly ImportTarget _oneOffTarget = ImportTarget.ForOneOffTrip();
    private ImportTarget? _shiftTarget;

    public ImportViewModel(MainViewModel main)
    {
        _main = main;
        IsLoading = true;
    }

    public ObservableCollection<LegLine> Legs { get; } = new ObservableCollection<LegLine>();

    /// <summary>Bestehende Schichten und "Neue Schicht"; die Einzelfahrt ist eine eigene Kachel</summary>
    public ObservableCollection<ImportTarget> Targets { get; } = new ObservableCollection<ImportTarget>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsReady))]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial ManualEntryViewModel? Manual { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsReady))]
    public partial bool IsManualStep { get; set; }

    [ObservableProperty]
    public partial bool CameFromManual { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsReady))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? Hint { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNewTarget))]
    [NotifyPropertyChangedFor(nameof(IsOneOffTarget))]
    [NotifyPropertyChangedFor(nameof(IsShiftMode))]
    [NotifyPropertyChangedFor(nameof(SaveHint))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial ImportTarget? SelectedTarget { get; set; }

    /// <summary>Aus dem Tab Fahrten gestartet: die Einzelfahrt ist gleich gewaehlt</summary>
    public bool PrefersOneOffTrip { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial string TripDateText { get; set; } = "";

    [ObservableProperty]
    public partial string TripWeekday { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTripDateError))]
    public partial string? TripDateError { get; set; }

    [ObservableProperty]
    public partial string TripName { get; set; } = "";

    [ObservableProperty]
    public partial string TripNamePlaceholder { get; set; } = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial string NewShiftName { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsReturn))]
    public partial bool IsOutbound { get; set; } = true;

    [ObservableProperty]
    public partial string? ReplacedSummary { get; set; }

    [ObservableProperty]
    public partial string NewSummary { get; set; } = "";

    public bool IsReturn
    {
        get { return !IsOutbound; }
    }

    public bool HasError
    {
        get { return ErrorMessage is not null; }
    }

    public bool IsReady
    {
        get { return !IsLoading && ErrorMessage is null && !IsManualStep; }
    }

    public bool IsNewTarget
    {
        get { return SelectedTarget is not null && SelectedTarget.IsNew; }
    }

    public bool IsOneOffTarget
    {
        get { return SelectedTarget is not null && SelectedTarget.IsOneOffTrip; }
    }

    /// <summary>Kachel Schicht gewaehlt: Schichtliste und Hin/Rueck sind zu sehen</summary>
    public bool IsShiftMode
    {
        get { return SelectedTarget is not null && !SelectedTarget.IsOneOffTrip; }
    }

    public bool HasTripDateError
    {
        get { return TripDateError is not null; }
    }

    public string SaveHint
    {
        get
        {
            if (IsOneOffTarget)
            {
                return UiStrings.Import_OneOffHint;
            }

            return UiStrings.Import_PatternHint;
        }
    }

    public bool HasReplacement
    {
        get { return ReplacedSummary is not null; }
    }

    public async Task LoadAsync(string sharedText)
    {
        try
        {
            var resolved = await _main.Services.Resolver.ResolveAsync(sharedText, CancellationToken.None);
            _connection = resolved.Connection;
            _sharedDate = resolved.TravelDate;
            foreach (var leg in resolved.Connection.Legs)
            {
                Legs.Add(new LegLine(leg));
            }

            NewSummary = ConnectionText.Summary(resolved.Connection);
            if (!resolved.HasPlatforms)
            {
                Hint = UiStrings.Import_NoPlatforms;
            }

            PrepareTripFields();
            FillTargets();
        }
        catch (ImportException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            ErrorMessage = UiLanguage.Format(UiStrings.Import_Failed, exception.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void StartManual()
    {
        if (Manual is null)
        {
            Manual = new ManualEntryViewModel(_main, UseManualConnection);
        }

        IsLoading = false;
        ErrorMessage = null;
        IsManualStep = true;
    }

    [RelayCommand]
    private void EditManual()
    {
        StartManual();
    }

    private void UseManualConnection(SavedConnection connection)
    {
        _connection = connection;
        Legs.Clear();
        foreach (var leg in connection.Legs)
        {
            Legs.Add(new LegLine(leg));
        }

        NewSummary = ConnectionText.Summary(connection);
        Hint = UiStrings.Import_ManualHint;
        CameFromManual = true;
        IsManualStep = false;
        PrepareTripFields();
        FillTargets();
    }

    [RelayCommand]
    private void SelectTarget(ImportTarget target)
    {
        ApplyTarget(target);
        if (target.Shift is not null)
        {
            var hasOutbound = target.Shift.Outbound is not null;
            var hasReturn = target.Shift.Return is not null;
            if (hasOutbound && !hasReturn)
            {
                IsOutbound = false;
            }
            else if (!hasOutbound)
            {
                IsOutbound = true;
            }
        }

        UpdateReplacement();
    }

    /// <summary>Zurueck zur Schicht, die vorher gewaehlt war; Hin/Rueck bleibt, wie es war</summary>
    [RelayCommand]
    private void ChooseShiftMode()
    {
        if (IsShiftMode || _shiftTarget is null)
        {
            return;
        }

        ApplyTarget(_shiftTarget);
        UpdateReplacement();
    }

    [RelayCommand]
    private void ChooseOneOffMode()
    {
        if (IsOneOffTarget)
        {
            return;
        }

        ApplyTarget(_oneOffTarget);
        UpdateReplacement();
    }

    private void ApplyTarget(ImportTarget target)
    {
        if (!target.IsOneOffTrip)
        {
            _shiftTarget = target;
        }

        foreach (var item in Targets)
        {
            item.IsSelected = item == target;
        }

        _oneOffTarget.IsSelected = target == _oneOffTarget;
        SelectedTarget = target;
    }

    [RelayCommand]
    private void ChooseOutbound()
    {
        IsOutbound = true;
        UpdateReplacement();
    }

    [RelayCommand]
    private void ChooseReturn()
    {
        IsOutbound = false;
        UpdateReplacement();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        if (_connection is null || SelectedTarget is null)
        {
            return;
        }

        if (SelectedTarget.IsOneOffTrip)
        {
            SaveOneOffTrip(_connection);
            return;
        }

        var data = _main.Services.Data;
        var shift = SelectedTarget.Shift;
        if (shift is null)
        {
            shift = new Shift { Name = NewShiftName.Trim() };
            var usedColors = data.Shifts.Select(item => item.ColorHex).ToList();
            var freeColor = ColorOption.ShiftColors.FirstOrDefault(color => !usedColors.Contains(color));
            if (freeColor is not null)
            {
                shift.ColorHex = freeColor;
            }

            data.Shifts.Add(shift);
        }

        shift.SetConnection(Direction, _connection);
        _main.Services.SaveAndReschedule();
        _main.CloseImport();
        _main.ShowToast(UiLanguage.Format(UiStrings.Import_Saved, Core.Checking.AlertComposer.DirectionName(Direction), shift.Name), false);
        _main.Navigate("shifts");
    }

    private void SaveOneOffTrip(SavedConnection connection)
    {
        var date = DateText.Parse(TripDateText);
        if (date is null)
        {
            return;
        }

        var name = TripName.Trim();
        if (name.Length == 0)
        {
            name = OneOffTrip.DefaultName(connection);
        }

        var trip = new OneOffTrip
        {
            Name = name,
            TravelDate = date.Value,
            Connection = connection,
            CreatedAt = _main.Services.Clock.Now
        };
        _main.Services.Data.Trips.Add(trip);
        _main.Services.SaveAndReschedule();
        _main.CloseImport();
        var dayText = Language.Culture.DateTimeFormat.GetAbbreviatedDayName(date.Value.DayOfWeek) + " " + date.Value.ToString(UiStrings.Format_DayMonth, Language.Culture);
        _main.ShowToast(UiLanguage.Format(UiStrings.Import_SavedOneOff, dayText), false);
        _main.Navigate("trips");
    }

    [RelayCommand]
    private void Close()
    {
        _main.CloseImport();
    }

    partial void OnTripDateTextChanged(string value)
    {
        UpdateTripDate();
    }

    private void PrepareTripFields()
    {
        if (_connection is null)
        {
            return;
        }

        TripNamePlaceholder = OneOffTrip.DefaultName(_connection);
        if (TripDateText.Length == 0)
        {
            var date = _sharedDate ?? Today();
            TripDateText = DateText.Format(date);
        }

        UpdateTripDate();
    }

    /// <summary>Wochentag neben dem Datum und Hinweis, wenn das Datum nicht passt; halb getippt gibt es noch keinen Hinweis</summary>
    private void UpdateTripDate()
    {
        var date = DateText.Parse(TripDateText);
        TripWeekday = "";
        if (date is not null)
        {
            TripWeekday = Language.Culture.DateTimeFormat.GetDayName(date.Value.DayOfWeek);
        }

        string? error = null;
        switch (TripDateProblemNow(date))
        {
            case TripDateProblem.Invalid:
                if (TripDateText.Length >= 10)
                {
                    error = UiStrings.Import_DateInvalid;
                }

                break;
            case TripDateProblem.Past:
                if (HasFullYear(TripDateText))
                {
                    error = UiStrings.Import_DatePast;
                }

                break;
            case TripDateProblem.AlreadyOver:
                error = UiStrings.Import_DateOver;
                break;
        }

        TripDateError = error;
        SaveCommand.NotifyCanExecuteChanged();
    }

    /// <remarks>Waehrend "23.10.20" noch getippt wird, waere das Jahr 2020 und der Hinweis "vorbei" falsch.</remarks>
    private static bool HasFullYear(string text)
    {
        var lastDot = text.LastIndexOf('.');
        return lastDot >= 0 && text.Length - lastDot - 1 >= 4;
    }

    private TripDateProblem TripDateProblemNow(DateOnly? date)
    {
        if (_connection is null)
        {
            return TripDateProblem.Invalid;
        }

        return OneOffTripDate.Check(date, _connection, _main.Services.Clock.Now, BerlinTime.Zone);
    }

    private DateOnly Today()
    {
        return DateOnly.FromDateTime(BerlinTime.ToWallClock(_main.Services.Clock.Now, BerlinTime.Zone));
    }

    private TripDirection Direction
    {
        get
        {
            if (IsOutbound)
            {
                return TripDirection.Outbound;
            }

            return TripDirection.Return;
        }
    }

    private bool CanSave()
    {
        if (_connection is null || SelectedTarget is null)
        {
            return false;
        }

        if (SelectedTarget.IsNew)
        {
            return !string.IsNullOrWhiteSpace(NewShiftName);
        }

        if (SelectedTarget.IsOneOffTrip)
        {
            return TripDateProblemNow(DateText.Parse(TripDateText)) == TripDateProblem.None;
        }

        return true;
    }

    private void FillTargets()
    {
        if (Targets.Count > 0)
        {
            UpdateReplacement();
            SaveCommand.NotifyCanExecuteChanged();
            return;
        }

        Targets.Clear();
        foreach (var shift in _main.Services.Data.Shifts)
        {
            Targets.Add(new ImportTarget(shift));
        }

        var newTarget = new ImportTarget(null);
        Targets.Add(newTarget);
        SelectTarget(newTarget);
        if (PrefersOneOffTrip)
        {
            ChooseOneOffMode();
        }
    }

    private void UpdateReplacement()
    {
        string? replaced = null;
        if (SelectedTarget is not null && SelectedTarget.Shift is not null)
        {
            var existing = SelectedTarget.Shift.GetConnection(Direction);
            if (existing is not null)
            {
                replaced = ConnectionText.Summary(existing);
            }
        }

        ReplacedSummary = replaced;
        OnPropertyChanged(nameof(HasReplacement));
    }
}
