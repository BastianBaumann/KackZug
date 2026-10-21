using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Models;
using KackZug.Localization;

namespace KackZug.ViewModels;

/// <summary>Bearbeiten einer Schicht: Name, Farbe, Fahrten, Loeschen</summary>
public partial class ShiftEditorViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private readonly Shift _shift;
    private readonly bool _isNew;
    private readonly Action _closed;

    public ShiftEditorViewModel(MainViewModel main, Shift shift, bool isNew, Action closed)
    {
        _main = main;
        _shift = shift;
        _isNew = isNew;
        _closed = closed;
        Name = shift.Name;

        foreach (var hex in ColorOption.ShiftColors)
        {
            var option = new ColorOption(hex);
            option.IsSelected = string.Equals(hex, shift.ColorHex, StringComparison.OrdinalIgnoreCase);
            Colors.Add(option);
        }

        Outbound = new ConnectionCardViewModel(shift, TripDirection.Outbound, MarkChanged);
        Return = new ConnectionCardViewModel(shift, TripDirection.Return, MarkChanged);
    }

    public ObservableCollection<ColorOption> Colors { get; } = new ObservableCollection<ColorOption>();

    public ConnectionCardViewModel Outbound { get; }

    public ConnectionCardViewModel Return { get; }

    public string Heading
    {
        get
        {
            if (_isNew)
            {
                return UiStrings.Shifts_New;
            }

            return UiStrings.Shifts_Edit;
        }
    }

    public bool CanDelete
    {
        get { return !_isNew; }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial bool HasChanges { get; set; }

    [RelayCommand]
    private void SelectColor(ColorOption option)
    {
        foreach (var color in Colors)
        {
            color.IsSelected = color == option;
        }

        MarkChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        _shift.Name = Name.Trim();
        var selected = Colors.FirstOrDefault(color => color.IsSelected);
        if (selected is not null)
        {
            _shift.ColorHex = selected.Hex;
        }

        var data = _main.Services.Data;
        if (_isNew && !data.Shifts.Contains(_shift))
        {
            data.Shifts.Add(_shift);
        }

        _main.Services.SaveAndReschedule();
        _main.ShowToast(UiStrings.Shifts_Saved, false);
        _closed();
    }

    [RelayCommand]
    private void Cancel()
    {
        if (!_isNew)
        {
            _main.Services.Reload();
        }

        _closed();
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        var confirmed = await _main.ConfirmAsync(
            UiStrings.Shifts_DeleteTitle,
            UiLanguage.Format(UiStrings.Shifts_DeleteMessage, _shift.Name),
            UiStrings.Shifts_DeleteConfirm);
        if (!confirmed)
        {
            return;
        }

        _main.Services.Data.RemoveShift(_shift.Id);
        _main.Services.SaveAndReschedule();
        _main.ShowToast(UiStrings.Shifts_Deleted, false);
        _closed();
    }

    partial void OnNameChanged(string value)
    {
        HasChanges = true;
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }

    private void MarkChanged()
    {
        HasChanges = true;
    }
}
