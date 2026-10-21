using KackZug.Core.Models;

namespace KackZug.Core.Planning;

/// <summary>Eine Fahrt an einem bestimmten Tag, die geprueft werden soll: aus einer Schicht oder eine Einzelfahrt</summary>
public sealed class TripPlan
{
    public TripPlan(DateOnly travelDate, Shift shift, TripDirection direction, SavedConnection connection)
    {
        TravelDate = travelDate;
        Shift = shift;
        Id = shift.Id;
        Name = shift.Name;
        Direction = direction;
        Connection = connection;
    }

    public TripPlan(OneOffTrip trip)
    {
        TravelDate = trip.TravelDate;
        OneOffTrip = trip;
        Id = trip.Id;
        Name = trip.Name;
        Direction = TripDirection.Outbound;
        Connection = trip.Connection;
    }

    public DateOnly TravelDate { get; }

    /// <summary>Id der Schicht oder der Einzelfahrt, so wie sie in Pruefungen und Pruefergebnissen steht</summary>
    public Guid Id { get; }

    public string Name { get; }

    public Shift? Shift { get; }

    public OneOffTrip? OneOffTrip { get; }

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
