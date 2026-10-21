using KackZug.Core.Import;
using KackZug.Core.Models;

namespace KackZug.Core.Tests.Import;

/// <summary>Prueft die Eingabe einer Verbindung von Hand</summary>
public class ManualConnectionBuilderTests
{
    private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 10, 21, 12, 0, 0, TimeSpan.FromHours(2));
    private static readonly Station Hbf = new Station { Name = "Musterstadt Hbf", EvaNumber = "8000001" };
    private static readonly Station Beispielhausen = new Station { Name = "Beispielhausen", EvaNumber = "8000002" };
    private static readonly Station Markt = new Station { Name = "Beispielhausen Markt", EvaNumber = "900001" };
    private static readonly Station Werkstor = new Station { Name = "Werkstor", EvaNumber = "900002" };

    [Fact]
    public void BuildsOneRide()
    {
        var input = Leg(Hbf, Beispielhausen, "6:42", "0710", " RE 7 ");
        input.TrainNumberText = "4711";

        var result = ManualConnectionBuilder.Build(new[] { input }, Now);

        Assert.Null(result.Error);
        var ride = Assert.Single(result.Connection!.Legs);
        Assert.Equal(new TimeOnly(6, 42), ride.PlannedDeparture);
        Assert.Equal(new TimeOnly(7, 10), ride.PlannedArrival);
        Assert.Equal("RE 7", ride.Name);
        Assert.Equal("RE 7", ride.Line);
        Assert.Equal("4711", ride.TrainNumber);
        Assert.Equal(Now, result.Connection.ImportedAt);
    }

    [Fact]
    public void AddsWalkBetweenDifferentStops()
    {
        var first = Leg(Hbf, Beispielhausen, "06:42", "07:10", "RE 7");
        var second = Leg(Markt, Werkstor, "07:20", "07:28", "STR 2");

        var connection = ManualConnectionBuilder.Build(new[] { first, second }, Now).Connection!;

        Assert.Equal(3, connection.Legs.Count);
        var walk = connection.Legs[1];
        Assert.True(walk.IsWalk);
        Assert.Equal("Beispielhausen", walk.From.Name);
        Assert.Equal("Beispielhausen Markt", walk.To.Name);
        Assert.Equal(new TimeOnly(7, 10), walk.PlannedDeparture);
        Assert.Equal(new TimeOnly(7, 20), walk.PlannedArrival);
    }

    [Fact]
    public void NoWalkWhenChangingAtTheSameStation()
    {
        var first = Leg(Hbf, Beispielhausen, "06:42", "07:10", "RE 7");
        var second = Leg(Beispielhausen, Werkstor, "07:15", "07:28", "Bus 11");

        var connection = ManualConnectionBuilder.Build(new[] { first, second }, Now).Connection!;

        Assert.Equal(2, connection.Legs.Count);
        Assert.All(connection.Legs, leg => Assert.False(leg.IsWalk));
    }

    [Fact]
    public void CountsDaysOverMidnight()
    {
        var first = Leg(Hbf, Beispielhausen, "23:40", "00:10", "RE 7");
        var second = Leg(Beispielhausen, Werkstor, "00:20", "00:35", "Bus 11");

        var connection = ManualConnectionBuilder.Build(new[] { first, second }, Now).Connection!;

        Assert.Equal(0, connection.Legs[0].DepartureDayOffset);
        Assert.Equal(1, connection.Legs[0].ArrivalDayOffset);
        Assert.Equal(1, connection.Legs[1].DepartureDayOffset);
        Assert.Equal(1, connection.Legs[1].ArrivalDayOffset);
    }

    [Fact]
    public void NamesTheMissingPart()
    {
        var missingStation = Leg(Hbf, null, "06:42", "07:10", "RE 7");
        var badTime = Leg(Hbf, Beispielhausen, "6 Uhr 42", "07:10", "RE 7");
        var noLine = Leg(Hbf, Beispielhausen, "06:42", "07:10", " ");
        var sameStation = Leg(Hbf, Hbf, "06:42", "07:10", "RE 7");
        var letters = Leg(Hbf, Beispielhausen, "06:42", "07:10", "RE 7");
        letters.TrainNumberText = "RE4711";

        Assert.Contains("Ziel", ManualConnectionBuilder.Build(new[] { missingStation }, Now).Error);
        Assert.Contains("Abfahrt", ManualConnectionBuilder.Build(new[] { badTime }, Now).Error);
        Assert.Contains("Linie", ManualConnectionBuilder.Build(new[] { noLine }, Now).Error);
        Assert.Contains("gleich", ManualConnectionBuilder.Build(new[] { sameStation }, Now).Error);
        Assert.Contains("Ziffern", ManualConnectionBuilder.Build(new[] { letters }, Now).Error);
        Assert.Contains("Teilstrecke 2", ManualConnectionBuilder.Build(new[] { Leg(Hbf, Beispielhausen, "06:42", "07:10", "RE 7"), missingStation }, Now).Error);
        Assert.NotNull(ManualConnectionBuilder.Build(new List<ManualLegInput>(), Now).Error);
    }

    private static ManualLegInput Leg(Station? from, Station? to, string departure, string arrival, string line)
    {
        return new ManualLegInput
        {
            From = from,
            To = to,
            DepartureText = departure,
            ArrivalText = arrival,
            LineText = line
        };
    }
}
