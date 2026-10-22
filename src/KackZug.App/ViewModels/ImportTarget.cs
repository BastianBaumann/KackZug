using CommunityToolkit.Mvvm.ComponentModel;
using KackZug.Core.Models;
using KackZug.Localization;

namespace KackZug.ViewModels;

/// <summary>Moegliches Ziel eines Imports: eine bestehende Schicht, eine neue oder eine Einzelfahrt</summary>
public partial class ImportTarget : ObservableObject
{
    public ImportTarget(Shift? shift)
        : this(shift, false)
    {
    }

    private ImportTarget(Shift? shift, bool isOneOffTrip)
    {
        Shift = shift;
        IsOneOffTrip = isOneOffTrip;
        if (isOneOffTrip)
        {
            Title = UiStrings.Import_TargetOneOff;
        }
        else if (shift is null)
        {
            Title = UiStrings.Shifts_New;
        }
        else
        {
            Title = shift.Name;
        }
    }

    public static ImportTarget ForOneOffTrip()
    {
        return new ImportTarget(null, true);
    }

    public Shift? Shift { get; }

    public bool IsOneOffTrip { get; }

    public string Title { get; }

    public bool IsNew
    {
        get { return Shift is null && !IsOneOffTrip; }
    }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
