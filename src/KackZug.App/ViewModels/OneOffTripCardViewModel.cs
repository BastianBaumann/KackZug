using KackZug.Core.Localization;
using KackZug.Core.Models;
using KackZug.Localization;

namespace KackZug.ViewModels;

/// <summary>Eine Einzelfahrt in der Liste: Name, Tag, Zeiten, letzter Stand</summary>
public sealed class OneOffTripCardViewModel
{
    public OneOffTripCardViewModel(MainViewModel main, OneOffTrip trip, DateOnly today)
    {
        Trip = trip;
        Name = trip.Name;
        var culture = Language.Culture;
        var date = trip.TravelDate;
        DateText = culture.DateTimeFormat.GetDayName(date.DayOfWeek) + ", " + date.ToString(UiStrings.Format_Date, culture);
        ShortDateText = culture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek) + " " + date.ToString(UiStrings.Format_DayMonth, culture);
        Kicker = "";
        if (date == today)
        {
            Kicker = UiStrings.Week_Today;
        }
        else if (date == today.AddDays(1))
        {
            Kicker = UiStrings.Trips_Tomorrow;
        }

        var connection = trip.Connection;
        var first = connection.FirstRide!;
        var lastLeg = connection.Legs[connection.Legs.Count - 1];
        TimesText = first.PlannedDeparture.ToString("HH:mm") + " → " + lastLeg.PlannedArrival.ToString("HH:mm");
        if (lastLeg.ArrivalDayOffset > first.DepartureDayOffset)
        {
            TimesText += " (+" + (lastLeg.ArrivalDayOffset - first.DepartureDayOffset) + ")";
        }

        RouteText = connection.Legs[0].From.Name + " → " + lastLeg.To.Name;

        var state = main.Services.Data.CheckStates.FirstOrDefault(item => item.Date == date && item.ShiftId == trip.Id);
        Live = new TripCardViewModel(main, date, trip.Id, UiStrings.Trips_OneOff, TripDirection.Outbound, connection, state);
        CheckedText = "";
        if (state is not null)
        {
            CheckedText = UiLanguage.Format(UiStrings.Day_CheckedAt, StatusBadge.CheckedTime(state.LastCheckedAt, main.Services.Clock.Now));
        }
    }

    public OneOffTrip Trip { get; }

    public string Name { get; }

    public string DateText { get; }

    /// <summary>Kurzer Tag fuer Rueckfragen und Hinweise, z. B. Do 23.10.</summary>
    public string ShortDateText { get; }

    public string Kicker { get; }

    public bool HasKicker
    {
        get { return Kicker.Length > 0; }
    }

    public string TimesText { get; }

    public string RouteText { get; }

    public string CheckedText { get; }

    /// <summary>Stand und Navigator-Knopf wie in der Detailansicht</summary>
    public TripCardViewModel Live { get; }
}
