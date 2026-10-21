using KackZug.Core.Bahn;
using KackZug.Core.Models;

namespace KackZug.Core.Tests.Links;

/// <summary>Prueft den Suchlink, mit dem eine Fahrt im DB Navigator geoeffnet wird</summary>
public class NavigatorLinkTests
{
    [Fact]
    public void UsesFirstAndLastRideWithTravelDate()
    {
        var shift = SampleData.CreateShift("Frueh");

        var link = NavigatorLink.ForTrip(shift.Outbound!, new DateOnly(2026, 10, 22));

        Assert.NotNull(link);
        Assert.StartsWith("https://www.bahn.de/buchung/fahrplan/suche#", link);
        Assert.Contains("so=Musterstadt%20Hbf", link);
        Assert.Contains("zo=Werkstor", link);
        Assert.Contains("soid=A%3D1%40O%3DMusterstadt%20Hbf%40L%3D8000001%40", link);
        Assert.Contains("zoid=A%3D1%40O%3DWerkstor%40L%3D900002%40", link);
        Assert.Contains("hd=2026-10-22T06%3A42%3A00", link);
        Assert.Contains("hza=D", link);
    }

    [Fact]
    public void EncodesUmlautsAndLeavesOutMissingNumbers()
    {
        var connection = new SavedConnection();
        connection.Legs.Add(new Leg
        {
            From = new Station { Name = "Köln Hbf", EvaNumber = "8000207" },
            To = new Station { Name = "Brücke (Nord)" },
            PlannedDeparture = new TimeOnly(23, 50),
            DepartureDayOffset = 1
        });

        var link = NavigatorLink.ForTrip(connection, new DateOnly(2026, 12, 31));

        Assert.Contains("so=K%C3%B6ln%20Hbf", link);
        Assert.Contains("zo=Br%C3%BCcke%20%28Nord%29", link);
        Assert.DoesNotContain("zoid=", link);
        Assert.Contains("hd=2027-01-01T23%3A50%3A00", link);
    }

    [Fact]
    public void ReturnsNullWithoutRides()
    {
        var connection = new SavedConnection();
        connection.Legs.Add(new Leg { Kind = LegKind.Walk, Name = "Fussweg" });

        Assert.Null(NavigatorLink.ForTrip(connection, new DateOnly(2026, 10, 22)));
    }
}
