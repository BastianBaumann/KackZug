using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Import;
using KackZug.Core.Models;

namespace KackZug.ViewModels;

/// <summary>Dialog nach dem Teilen: Verbindung laden und einer Schicht als Hin- oder Rueckfahrt zuordnen</summary>
public partial class ImportViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private SavedConnection? _connection;

    public ImportViewModel(MainViewModel main)
    {
        _main = main;
        IsLoading = true;
    }

    public ObservableCollection<LegLine> Legs { get; } = new ObservableCollection<LegLine>();

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
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial ImportTarget? SelectedTarget { get; set; }

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
            foreach (var leg in resolved.Connection.Legs)
            {
                Legs.Add(new LegLine(leg));
            }

            NewSummary = ConnectionText.Summary(resolved.Connection);
            if (!resolved.HasPlatforms)
            {
                Hint = "Gleise konnten gerade nicht geladen werden. Gleiswechsel werden für diese Fahrt nicht gemeldet.";
            }

            FillTargets();
        }
        catch (ImportException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            ErrorMessage = "Die Verbindung konnte nicht übernommen werden: " + exception.Message;
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
        Hint = "Von Hand eingegeben: Gleiswechsel kann KackZug dafür nicht melden, Verspätung und Ausfall schon.";
        CameFromManual = true;
        IsManualStep = false;
        FillTargets();
    }

    [RelayCommand]
    private void SelectTarget(ImportTarget target)
    {
        foreach (var item in Targets)
        {
            item.IsSelected = item == target;
        }

        SelectedTarget = target;
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
        _main.ShowToast(Core.Checking.AlertComposer.DirectionName(Direction) + " für \"" + shift.Name + "\" gespeichert.", false);
        _main.Navigate("shifts");
    }

    [RelayCommand]
    private void Close()
    {
        _main.CloseImport();
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
