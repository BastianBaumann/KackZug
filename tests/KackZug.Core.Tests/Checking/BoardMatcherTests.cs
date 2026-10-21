using KackZug.Core.Checking;
using KackZug.Core.Models;

namespace KackZug.Core.Tests.Checking;

/// <summary>Prueft das Finden des gespeicherten Zugs auf der Abfahrtstafel</summary>
public class BoardMatcherTests
{
    private static readonly DateTime Departure = new DateTime(2026, 10, 21, 17, 38, 0);

    private static readonly List<BoardEntry> Board = new List<BoardEntry>
    {
        new BoardEntry { PlannedTime = Departure, JourneyId = "a", Name = "RE1", LineNumber = "RE1", Category = "RE", Number = "3664" },
        new BoardEntry { PlannedTime = Departure, JourneyId = "b", Name = "Bus 10", LineNumber = "10", Category = "Bus", Number = "10" }
    };

    [Theory]
    [InlineData("RE 1", "a")]
    [InlineData("re1", "a")]
    [InlineData("Bus 10", "b")]
    [InlineData("10", "b")]
    public void FindsHandEnteredLines(string line, string journeyId)
    {
        var leg = new Leg { Name = line, Line = line, PlannedDeparture = new TimeOnly(17, 38) };

        var entry = BoardMatcher.Find(Board, leg, Departure);

        Assert.NotNull(entry);
        Assert.Equal(journeyId, entry.JourneyId);
    }

    [Fact]
    public void FindsNothingForUnknownLine()
    {
        var leg = new Leg { Name = "RB 99", Line = "RB 99", PlannedDeparture = new TimeOnly(17, 38) };

        Assert.Null(BoardMatcher.Find(Board, leg, Departure));
    }
}
