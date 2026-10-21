using KackZug.Core.Models;

namespace KackZug.Core.Planning;

/// <summary>Prueft das Datum einer neuen Einzelfahrt, bevor sie gespeichert wird</summary>
public static class OneOffTripDate
{
    public static TripDateProblem Check(DateOnly? date, SavedConnection connection, DateTimeOffset now, TimeZoneInfo zone)
    {
        if (date is null)
        {
            return TripDateProblem.Invalid;
        }

        var today = DateOnly.FromDateTime(BerlinTime.ToWallClock(now, zone));
        if (date.Value < today)
        {
            return TripDateProblem.Past;
        }

        if (IsOver(connection, date.Value, now, zone))
        {
            return TripDateProblem.AlreadyOver;
        }

        return TripDateProblem.None;
    }

    /// <summary>Vorbei ist eine Fahrt eine Stunde nach der geplanten Ankunft, damit Verspaetungen noch Platz haben</summary>
    public static bool IsOver(SavedConnection connection, DateOnly travelDate, DateTimeOffset now, TimeZoneInfo zone)
    {
        var arrival = BerlinTime.ToInstant(OneOffTrip.ArrivalOf(connection, travelDate), zone);
        return now >= arrival + OneOffTrip.KeepAfterArrival;
    }
}
