using System.Text;
using KackZug.Core.Localization;
using KackZug.Core.Models;

namespace KackZug.Core.Checking;

/// <summary>Schreibt aus einem Pruefergebnis eine kurze, konkrete Meldung in der eingestellten Sprache</summary>
public static class AlertComposer
{
    public static TripAlert Compose(Shift shift, TripDirection direction, DateOnly travelDate, TripStatus status)
    {
        var name = shift.Name + " " + DirectionName(direction);
        var shortName = shift.Name + " " + ShortDirectionName(direction);
        var kicker = shift.Name + " · " + DirectionName(direction);
        return Compose(shift.Id, direction, travelDate, status, name, shortName, kicker);
    }

    /// <summary>Meldung zu einer Einzelfahrt: nur ihr Name, ohne Hin- oder Rueckfahrt</summary>
    public static TripAlert Compose(OneOffTrip trip, TripStatus status)
    {
        return Compose(trip.Id, TripDirection.Outbound, trip.TravelDate, status, trip.Name, trip.Name, trip.Name);
    }

    private static TripAlert Compose(Guid id, TripDirection direction, DateOnly travelDate, TripStatus status, string name, string shortName, string kicker)
    {
        var alert = new TripAlert
        {
            TravelDate = travelDate,
            ShiftId = id,
            Direction = direction,
            Status = status.Overall,
            Severity = SeverityOf(status.Overall),
            StatusHash = HashOf(status)
        };

        var headline = Headline(status);
        alert.Title = name + ": " + headline;
        if (alert.Severity == AlertSeverity.Problem)
        {
            alert.Title = (shortName + ": " + headline).ToUpper(Language.Culture);
        }

        alert.BadgeText = BadgeText(status);
        alert.Text = Details(status);
        alert.Kicker = Kicker(kicker, travelDate);
        alert.Headline = AlarmHeadline(status);
        alert.TripLine = TripLine(status);
        return alert;
    }

    public static string DirectionName(TripDirection direction)
    {
        if (direction == TripDirection.Outbound)
        {
            return Strings.Alert_DirectionOutbound;
        }

        return Strings.Alert_DirectionReturn;
    }

    public static string ShortDirectionName(TripDirection direction)
    {
        if (direction == TripDirection.Outbound)
        {
            return Strings.Alert_DirectionOutboundShort;
        }

        return Strings.Alert_DirectionReturnShort;
    }

    public static string BadgeText(TripStatus status)
    {
        switch (status.Overall)
        {
            case LegStatus.OnTime:
                return Strings.Alert_BadgeOnTime;
            case LegStatus.PlannedOnly:
                return Strings.Alert_BadgePlannedOnly;
            case LegStatus.Unknown:
                return Strings.Alert_BadgeUnknown;
            case LegStatus.Delayed:
                var delay = status.Legs.Max(result => result.DelayMinutes);
                return Format(Strings.Alert_BadgeDelay, delay);
            case LegStatus.PlatformChanged:
                var changed = status.Legs.First(result => result.NewPlatform is not null);
                if (string.IsNullOrEmpty(changed.Leg.PlannedPlatform))
                {
                    return Format(Strings.Alert_BadgePlatform, changed.NewPlatform);
                }

                return Format(Strings.Alert_BadgePlatformChange, changed.Leg.PlannedPlatform, changed.NewPlatform);
            case LegStatus.TransferAtRisk:
                var lateLeg = DelayedLeg(status);
                if (lateLeg is not null)
                {
                    return Format(Strings.Alert_BadgeTransferAtRiskDelay, lateLeg.DelayMinutes);
                }

                return Strings.Alert_BadgeTransferAtRisk;
            case LegStatus.NotFound:
                return Strings.Alert_BadgeNotFound;
            case LegStatus.PartlyCancelled:
                return Strings.Alert_BadgePartlyCancelled;
            default:
                return Strings.Alert_BadgeCancelled;
        }
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

    private static string Kicker(string name, DateOnly travelDate)
    {
        var day = Language.Culture.DateTimeFormat.GetAbbreviatedDayName(travelDate.DayOfWeek);
        var text = name + " · " + Format(Strings.Alert_KickerDate, day, travelDate);
        return text.ToUpper(Language.Culture);
    }

    private static string AlarmHeadline(TripStatus status)
    {
        switch (status.Overall)
        {
            case LegStatus.Cancelled:
                return Strings.Alert_HeadlineCancelled;
            case LegStatus.PartlyCancelled:
                return Strings.Alert_HeadlinePartlyCancelled;
            case LegStatus.NotFound:
                return Strings.Alert_HeadlineNotFound;
            case LegStatus.TransferAtRisk:
                return Strings.Alert_HeadlineTransferAtRisk;
            case LegStatus.Delayed:
                var delay = status.Legs.Max(result => result.DelayMinutes);
                return Format(Strings.Alert_HeadlineDelay, delay);
            case LegStatus.PlatformChanged:
                return Strings.Alert_HeadlinePlatformChanged;
            case LegStatus.Unknown:
                return Strings.Alert_HeadlineUnknown;
            case LegStatus.PlannedOnly:
                return Strings.Alert_HeadlinePlannedOnly;
            default:
                return Strings.Alert_HeadlineOnTime;
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

        var line = leg.Leg.Name + " · " + leg.Leg.PlannedDeparture.ToString("HH:mm") + " " + leg.Leg.From.Name + " → " + leg.Leg.To.Name;
        if (status.Overall == LegStatus.TransferAtRisk && leg.Status == LegStatus.Delayed)
        {
            line += " · " + Format(Strings.Alert_BadgeDelay, leg.DelayMinutes);
        }

        return line;
    }

    private static string Headline(TripStatus status)
    {
        var worstIndex = WorstLegIndex(status);
        if (status.Overall == LegStatus.TransferAtRisk)
        {
            var transfer = status.Transfers.First(item => item.AtRisk);
            var station = status.Legs[transfer.ArrivingLegIndex].Leg.To.Name;
            var lateLeg = DelayedLeg(status);
            if (lateLeg is not null)
            {
                return Format(Strings.Alert_TitleTransferDelayed, Describe(lateLeg), lateLeg.DelayMinutes, station);
            }

            return Format(Strings.Alert_TitleTransferAtRisk, station);
        }

        if (worstIndex < 0)
        {
            var firstRide = status.Legs.First(result => !result.Leg.IsWalk);
            if (status.Overall == LegStatus.PlannedOnly)
            {
                return Format(Strings.Alert_TitlePlannedOnly, Describe(firstRide));
            }

            return Format(Strings.Alert_TitleOnTime, Describe(firstRide));
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
                problem = Strings.Alert_ProblemPlannedOnly;
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
            lines.Add(Format(Strings.Alert_DetailTransfer, station, spare));
        }

        if (status.Overall == LegStatus.PlannedOnly)
        {
            lines.Add(Strings.Alert_DetailPlannedOnly);
        }

        if (status.Overall == LegStatus.NotFound || status.Overall == LegStatus.Cancelled || status.Overall == LegStatus.PartlyCancelled)
        {
            lines.Add(Strings.Alert_DetailReplacement);
        }

        return string.Join("\n", lines);
    }

    private static LegResult? DelayedLeg(TripStatus status)
    {
        LegResult? latest = null;
        foreach (var result in status.Legs)
        {
            if (result.Status != LegStatus.Delayed)
            {
                continue;
            }

            if (latest is null || result.DelayMinutes > latest.DelayMinutes)
            {
                latest = result;
            }
        }

        return latest;
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
                return Strings.Alert_ProblemCancelled;
            case LegStatus.PartlyCancelled:
                return Format(Strings.Alert_ProblemPartlyCancelled, result.Leg.To.Name);
            case LegStatus.NotFound:
                return Strings.Alert_ProblemNotFound;
            case LegStatus.Delayed:
                return Format(Strings.Alert_ProblemDelay, result.DelayMinutes);
            case LegStatus.PlatformChanged:
                return Format(Strings.Alert_ProblemPlatform, result.NewPlatform);
            case LegStatus.Unknown:
                return Strings.Alert_ProblemUnknown;
            default:
                return Strings.Alert_ProblemOnTime;
        }
    }

    private static string Format(string text, params object?[] values)
    {
        return string.Format(Language.Culture, text, values);
    }
}
