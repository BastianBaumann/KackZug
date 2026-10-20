using KackZug.Core.Models;

namespace KackZug.Core.Tests;

/// <summary>Erfundene Beispieldaten fuer Tests</summary>
public static class SampleData
{
    public static Shift CreateShift(string name)
    {
        var outbound = new SavedConnection();
        outbound.Legs.Add(new Leg
        {
            From = new Station { Name = "Musterstadt Hbf", EvaNumber = "8000001" },
            To = new Station { Name = "Beispielhausen", EvaNumber = "8000002" },
            PlannedDeparture = new TimeOnly(6, 42),
            PlannedArrival = new TimeOnly(7, 10),
            Name = "RE7",
            Category = "NRE",
            Line = "7",
            TrainNumber = "4711",
            PlannedPlatform = "3"
        });
        outbound.Legs.Add(new Leg
        {
            Kind = LegKind.Walk,
            From = new Station { Name = "Beispielhausen", EvaNumber = "8000002" },
            To = new Station { Name = "Beispielhausen Markt", EvaNumber = "900001" },
            PlannedDeparture = new TimeOnly(7, 10),
            PlannedArrival = new TimeOnly(7, 15),
            Name = "Fussweg"
        });
        outbound.Legs.Add(new Leg
        {
            From = new Station { Name = "Beispielhausen Markt", EvaNumber = "900001" },
            To = new Station { Name = "Werkstor", EvaNumber = "900002" },
            PlannedDeparture = new TimeOnly(7, 20),
            PlannedArrival = new TimeOnly(7, 28),
            Name = "STR 2",
            Category = "STR",
            Line = "2",
            TrainNumber = "2"
        });

        return new Shift { Name = name, Outbound = outbound };
    }
}
