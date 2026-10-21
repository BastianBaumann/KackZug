using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Models;
using KackZug.Localization;

namespace KackZug.ViewModels;

/// <summary>Liste der Schichten mit Hin- und Rueckfahrt</summary>
public partial class ShiftsViewModel : PageViewModel
{
    public ShiftsViewModel(MainViewModel main)
    {
        Main = main;
        main.Services.DataChanged += (_, _) => Refresh();
        Refresh();
    }

    public MainViewModel Main { get; }

    public override string Title
    {
        get { return UiStrings.Nav_Shifts; }
    }

    public ObservableCollection<ShiftCardViewModel> Shifts { get; } = new ObservableCollection<ShiftCardViewModel>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing))]
    [NotifyPropertyChangedFor(nameof(IsListVisible))]
    public partial ShiftEditorViewModel? Editor { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    public bool IsEditing
    {
        get { return Editor is not null; }
    }

    public bool IsListVisible
    {
        get { return Editor is null; }
    }

    public override void OnShown()
    {
    }

    public bool HandleBack()
    {
        if (Editor is null)
        {
            return false;
        }

        Editor.CancelCommand.Execute(null);
        return true;
    }

    public void OpenEditor(Shift shift, bool isNew)
    {
        Editor = new ShiftEditorViewModel(Main, shift, isNew, CloseEditor);
    }

    [RelayCommand]
    private void Edit(ShiftCardViewModel card)
    {
        OpenEditor(card.Shift, false);
    }

    [RelayCommand]
    private void EnterManually()
    {
        Main.StartManualEntry();
    }

    [RelayCommand]
    private void Add()
    {
        var shift = new Shift { Name = "" };
        var usedColors = Main.Services.Data.Shifts.Select(item => item.ColorHex).ToList();
        var freeColor = ColorOption.ShiftColors.FirstOrDefault(color => !usedColors.Contains(color));
        if (freeColor is not null)
        {
            shift.ColorHex = freeColor;
        }

        OpenEditor(shift, true);
    }

    private void CloseEditor()
    {
        Editor = null;
        Refresh();
    }

    private void Refresh()
    {
        Shifts.Clear();
        foreach (var shift in Main.Services.Data.Shifts)
        {
            Shifts.Add(new ShiftCardViewModel(shift));
        }

        IsEmpty = Shifts.Count == 0;
    }
}
