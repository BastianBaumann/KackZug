using KackZug.Core.Models;

namespace KackZug.Core.Planning;

/// <summary>Eine Fahrt an einem bestimmten Tag, die geprueft werden soll</summary>
public sealed class TripPlan
{
    public TripPlan(DateOnly travelDate, Shift shift, TripDirection direction, SavedConnection connection)
    {
        TravelDate = travelDate;
        Shift = shift;
        Direction = direction;
        Connection = connection;
    }

    public DateOnly TravelDate { get; }

    public Shift Shift { get; }

    public TripDirection Direction { get; }

    public SavedConnection Connection { get; }

    public DateTime FirstDeparture
    {
        get
        {
            var firstRide = Connection.FirstRide;
            if (firstRide is null)
            {
                return TravelDate.ToDateTime(TimeOnly.MinValue);
            }

            return firstRide.DepartureOn(TravelDate);
        }
    }
}
