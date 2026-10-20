namespace KackZug.Core.Models;

/// <summary>Plan fuer einen Wochentag: welche Schicht und welche Fahrten geprueft werden</summary>
public sealed class DayPlan
{
    public DayOfWeek Day { get; set; }

    public Guid? ShiftId { get; set; }

    public bool OutboundEnabled { get; set; } = true;

    public bool ReturnEnabled { get; set; } = true;

    public bool IsTripEnabled(TripDirection direction)
    {
        if (direction == TripDirection.Outbound)
        {
            return OutboundEnabled;
        }

        return ReturnEnabled;
    }

    public DayPlan Copy()
    {
        return new DayPlan
        {
            Day = Day,
            ShiftId = ShiftId,
            OutboundEnabled = OutboundEnabled,
            ReturnEnabled = ReturnEnabled
        };
    }
}
