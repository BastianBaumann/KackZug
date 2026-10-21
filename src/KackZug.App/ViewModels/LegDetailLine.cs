using KackZug.Core.Checking;
using KackZug.Core.Models;
using KackZug.Localization;

namespace KackZug.ViewModels;

/// <summary>Eine Teilstrecke in der Fahrt-Detailansicht, mit Live-Stand falls bekannt</summary>
public sealed class LegDetailLine
{
    public LegDetailLine(Leg leg, LegResult? result)
    {
        var planned = new LegLine(leg);
        IsWalk = leg.IsWalk;
        Details = planned.Details;
        DepartureTime = planned.DepartureTime;
        ArrivalTime = planned.ArrivalTime;
        FromName = planned.FromName;
        ToName = planned.ToName;
        LineName = planned.LineName;
        Meta = planned.Meta;
        LiveText = "";

        if (result is null || result.Observation is null || leg.IsWalk)
        {
            return;
        }

        var observation = result.Observation;
        if (result.Status != LegStatus.OnTime && result.Status != LegStatus.PlannedOnly)
        {
            Badge = new StatusBadge(result.Status, BadgeText(result), "");
        }

        var parts = new List<string>();
        if (observation.LiveDeparture is not null && result.DelayMinutes > 0)
        {
            parts.Add(UiLanguage.Format(UiStrings.Leg_LiveDeparture, observation.LiveDeparture.Value.ToString("HH:mm")));
        }

        if (observation.LiveArrival is not null && observation.LiveArrival.Value > observation.PlannedArrival)
        {
            parts.Add(UiLanguage.Format(UiStrings.Leg_LiveArrival, observation.LiveArrival.Value.ToString("HH:mm")));
        }

        if (result.NewPlatform is not null)
        {
            parts.Add(UiLanguage.Format(UiStrings.Leg_NewPlatform, result.NewPlatform));
        }

        if (observation.Result == ObservationResult.Found && !observation.HasLiveData)
        {
            parts.Add(UiStrings.Leg_NoLiveData);
        }

        LiveText = string.Join(" · ", parts);
    }

    public bool IsWalk { get; }

    public string Details { get; }

    public string DepartureTime { get; }

    public string ArrivalTime { get; }

    public string FromName { get; }

    public string ToName { get; }

    public string LineName { get; }

    public string Meta { get; }

    public string LiveText { get; }

    public StatusBadge? Badge { get; }

    private static string BadgeText(LegResult result)
    {
        switch (result.Status)
        {
            case LegStatus.Delayed:
                return UiLanguage.Format(UiStrings.Leg_DelayMinutes, result.DelayMinutes);
            case LegStatus.PlatformChanged:
                return UiLanguage.Format(UiStrings.Leg_Platform, result.NewPlatform);
            default:
                return StatusBadge.ShortText(result.Status);
        }
    }
}
