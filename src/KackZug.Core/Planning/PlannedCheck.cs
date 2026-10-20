using KackZug.Core.Models;

namespace KackZug.Core.Planning;

/// <summary>Eine Pruefung zu einem festen Zeitpunkt, die Android als Alarm stellt</summary>
public sealed class PlannedCheck
{
    public CheckKind Kind { get; set; }

    public DateTimeOffset At { get; set; }

    public DateOnly TravelDate { get; set; }

    public Guid ShiftId { get; set; }

    public TripDirection Direction { get; set; }

    public int LeadMinutes { get; set; }

    public int RequestCode { get; set; }

    public string Key
    {
        get
        {
            if (Kind == CheckKind.Maintenance)
            {
                return "maintenance";
            }

            return Kind + "|" + TravelDate.ToString("yyyy-MM-dd") + "|" + ShiftId.ToString("N") + "|" + Direction + "|" + LeadMinutes;
        }
    }
}
