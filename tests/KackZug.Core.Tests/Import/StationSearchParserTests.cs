using System.Text.Json;
using KackZug.Core.Import;

namespace KackZug.Core.Tests.Import;

/// <summary>Prueft das Lesen der Ortssuche</summary>
public class StationSearchParserTests
{
    [Fact]
    public void KeepsStationsOnceAndDropsAddressesAndPlaces()
    {
        using var document = JsonDocument.Parse(Fixtures.Read("orte.json"));

        var stations = StationSearchParser.Parse(document.RootElement);

        Assert.Equal(2, stations.Count);
        Assert.Equal("Musterstadt Hbf", stations[0].Name);
        Assert.Equal("8000001", stations[0].EvaNumber);
        Assert.Equal("Musterstadt Nord", stations[1].Name);
    }

    [Fact]
    public void ReturnsEmptyListForUnexpectedAnswer()
    {
        using var document = JsonDocument.Parse("{\"fehler\":true}");

        Assert.Empty(StationSearchParser.Parse(document.RootElement));
    }
}
