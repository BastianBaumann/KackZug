namespace KackZug.Core.Models;

/// <summary>Alle gespeicherten Daten der App in einer Datei</summary>
public sealed class AppData
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public List<Shift> Shifts { get; set; } = new List<Shift>();

    public WeekPlan? CurrentWeek { get; set; }

    public Settings Settings { get; set; } = new Settings();

    public List<CheckState> CheckStates { get; set; } = new List<CheckState>();

    public DateTimeOffset? LastUpdateCheck { get; set; }

    public Shift? FindShift(Guid? shiftId)
    {
        if (shiftId is null)
        {
            return null;
        }

        return Shifts.FirstOrDefault(shift => shift.Id == shiftId.Value);
    }

    public void RemoveShift(Guid shiftId)
    {
        Shifts.RemoveAll(shift => shift.Id == shiftId);
        CheckStates.RemoveAll(state => state.ShiftId == shiftId);

        if (CurrentWeek is null)
        {
            return;
        }

        foreach (var day in CurrentWeek.Days)
        {
            if (day.ShiftId == shiftId)
            {
                day.ShiftId = null;
            }
        }
    }
}
