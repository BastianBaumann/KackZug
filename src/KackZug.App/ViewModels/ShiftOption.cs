using KackZug.Core.Models;
using KackZug.Localization;

namespace KackZug.ViewModels;

/// <summary>Eintrag in der Schichtauswahl eines Tages, ohne Schicht bedeutet frei</summary>
public sealed class ShiftOption
{
    public ShiftOption(Shift? shift)
    {
        Shift = shift;
        if (shift is null)
        {
            Name = UiStrings.Day_Free;
        }
        else
        {
            Name = shift.Name;
        }
    }

    public Shift? Shift { get; }

    public string Name { get; }

    public override string ToString()
    {
        return Name;
    }
}
