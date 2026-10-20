using System.Globalization;
using System.Text;
using KackZug.Core.Models;

namespace KackZug.Core.Checking;

/// <summary>Schreibt aus einem Pruefergebnis eine kurze, konkrete Meldung auf Deutsch</summary>
public static class AlertComposer
{
    public static TripAlert Compose(Shift shift, TripDirection direction, DateOnly travelDate, TripStatus status)
    {
        var alert = new TripAlert
        {
            TravelDate = travelDate,
            ShiftId = shift.Id,
            Direction = direction,
            Status = status.Overall,
            Severity = SeverityOf(status.Overall),
            StatusHash = HashOf(status)
        };

        var headline = Headline(status);
        alert.Title = shift.Name + " " + DirectionName(direction) + ": " + headline;
        alert.Text = Details(status);
        alert.Kicker = Kicker(shift, direction, travelDate);
        alert.Headline = AlarmHeadline(status);
        alert.TripLine = TripLine(status);
        return alert;
    }

    public static string DirectionName(TripDirection direction)
    {
        if (direction == TripDirection.Outbound)
        {
            return "Hinfahrt";
        }

        return "Rückfahrt";
    }

    public static AlertSeverity SeverityOf(LegStatus status)
    {
        if (status == LegStatus.OnTime || status == LegStatus.PlannedOnly || status == LegStatus.Unknown)
        {
            return AlertSeverity.Info;
        }

        return AlertSeverity.Problem;
    }

    public static string HashOf(TripStatus status)
    {
        var builder = new StringBuilder();
        builder.Append((int)status.Overall);
        foreach (var leg in status.Legs)
        {
            builder.Append('|');
            builder.Append((int)leg.Status);
            builder.Append(':');
            builder.Append(leg.DelayMinutes / 5);
            builder.Append(':');
            builder.Append(leg.NewPlatform);
        }

        foreach (var transfer in status.Transfers)
        {
            builder.Append("|T");
            builder.Append(transfer.AtRisk);
        }

        return builder.ToString();
    }

    private static readonly CultureInfo German = new CultureInfo("de-DE");

    private static string Kicker(Shift shift, TripDirection direction, DateOnly travelDate)
    {
        var day = German.DateTimeFormat.GetAbbreviatedDayName(travelDate.DayOfWeek);
        var text = shift.Name + " · " + DirectionName(direction) + " · " + day + " " + travelDate.ToString("dd.MM.", German);
        return text.ToUpper(German);
    }

    private static string AlarmHeadline(TripStatus status)
    {
        switch (status.Overall)
        {
            case LegStatus.Cancelled:
                return "FÄLLT AUS";
            case LegStatus.PartlyCancelled:
                return "TEILAUSFALL";
            case LegStatus.NotFound:
                return "NICHT GEFUNDEN";
            case LegStatus.TransferAtRisk:
                return "ANSCHLUSS GEFÄHRDET";
            case LegStatus.Delayed:
                var delay = status.Legs.Max(result => result.DelayMinutes);
                return "+" + delay + " MIN.";
            case LegStatus.PlatformChanged:
                return "GLEISWECHSEL";
            case LegStatus.Unknown:
                return "NICHT GEPRÜFT";
            case LegStatus.PlannedOnly:
                return "LAUT FAHRPLAN";
            default:
                return "PLANMÄSSIG";
        }
    }

    private static string TripLine(TripStatus status)
    {
        LegResult? leg = null;
        if (status.Overall == LegStatus.TransferAtRisk)
        {
            var transfer = status.Transfers.First(item => item.AtRisk);
            leg = status.Legs[transfer.ArrivingLegIndex];
        }
        else
        {
            var worstIndex = WorstLegIndex(status);
            if (worstIndex >= 0)
            {
                leg = status.Legs[worstIndex];
            }
            else
            {
                leg = status.Legs.First(result => !result.Leg.IsWalk);
            }
        }

        return leg.Leg.Name + " · " + leg.Leg.PlannedDeparture.ToString("HH:mm") + " " + leg.Leg.From.Name + " → " + leg.Leg.To.Name;
    }

    private static string Headline(TripStatus status)
    {
        var worstIndex = WorstLegIndex(status);
        if (status.Overall == LegStatus.TransferAtRisk)
        {
            var transfer = status.Transfers.First(item => item.AtRisk);
            var station = status.Legs[transfer.ArrivingLegIndex].Leg.To.Name;
            return "Anschluss in " + station + " gefährdet";
        }

        if (worstIndex < 0)
        {
            var firstRide = status.Legs.First(result => !result.Leg.IsWalk);
            if (status.Overall == LegStatus.PlannedOnly)
            {
                return Describe(firstRide) + " fährt laut Fahrplan";
            }

            return Describe(firstRide) + " fährt planmäßig";
        }

        return Describe(status.Legs[worstIndex]) + " " + Problem(status.Legs[worstIndex]);
    }

    private static string Details(TripStatus status)
    {
        var lines = new List<string>();
        for (var index = 0; index < status.Legs.Count; index++)
        {
            var result = status.Legs[index];
            if (result.Leg.IsWalk)
            {
                continue;
            }

            var problem = Problem(result);
            if (status.Overall == LegStatus.PlannedOnly)
            {
                problem = "laut Fahrplan";
            }

            var line = Describe(result) + " " + result.Leg.From.Name + " → " + result.Leg.To.Name + ": " + problem;
            lines.Add(line);
        }

        foreach (var transfer in status.Transfers)
        {
            if (!transfer.AtRisk)
            {
                continue;
            }

            var station = status.Legs[transfer.ArrivingLegIndex].Leg.To.Name;
            var spare = Math.Max(0, transfer.SpareMinutes);
            lines.Add("Umstieg in " + station + ": nur noch " + spare + " Min. Puffer");
        }

        if (status.Overall == LegStatus.PlannedOnly)
        {
            lines.Add("Echtzeitdaten gibt es erst kurz vor der Abfahrt. KackZug prüft dann noch einmal.");
        }

        if (status.Overall == LegStatus.NotFound || status.Overall == LegStatus.Cancelled || status.Overall == LegStatus.PartlyCancelled)
        {
            lines.Add("Eventuell gibt es Ersatzverkehr. Bitte im DB Navigator prüfen.");
        }

        return string.Join("\n", lines);
    }

    private static int WorstLegIndex(TripStatus status)
    {
        var worstIndex = -1;
        var worstStatus = LegStatus.OnTime;
        for (var index = 0; index < status.Legs.Count; index++)
        {
            var legStatus = status.Legs[index].Status;
            if ((int)legStatus > (int)worstStatus)
            {
                worstStatus = legStatus;
                worstIndex = index;
            }
        }

        return worstIndex;
    }

    private static string Describe(LegResult result)
    {
        return result.Leg.Name + " " + result.Leg.PlannedDeparture.ToString("HH:mm");
    }

    private static string Problem(LegResult result)
    {
        switch (result.Status)
        {
            case LegStatus.Cancelled:
                return "fällt aus";
            case LegStatus.PartlyCancelled:
                return "hält nicht in " + result.Leg.To.Name;
            case LegStatus.NotFound:
                return "fährt so nicht mehr";
            case LegStatus.Delayed:
                return "+" + result.DelayMinutes + " Min.";
            case LegStatus.PlatformChanged:
                return "jetzt Gleis " + result.NewPlatform;
            case LegStatus.Unknown:
                return "konnte nicht geprüft werden";
            default:
                return "pünktlich";
        }
    }
}
