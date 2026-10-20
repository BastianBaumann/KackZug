#if DEBUG
using KackZug.Core.Import;
using KackZug.Core.Models;

namespace KackZug.Desktop;

/// <summary>Erfundener Import fuer die Screenshot-Tour, damit keine Anfragen an bahn.de gehen</summary>
internal sealed class DemoResolver : IConnectionResolver
{
    public Task<ResolvedConnection> ResolveAsync(string sharedText, CancellationToken cancellationToken)
    {
        if (sharedText.Contains("kaputt", StringComparison.Ordinal))
        {
            throw new ImportException("Im geteilten Text wurde kein Link von bahn.de gefunden.");
        }

        var connection = new SavedConnection { ImportedAt = DateTimeOffset.Now };
        connection.Legs.Add(new Leg
        {
            Name = "RB21",
            Category = "DRB",
            TrainNumber = "80611",
            From = new Station { Name = "Beispielhausen", EvaNumber = "8000002" },
            To = new Station { Name = "Musterstadt Hbf", EvaNumber = "8000001" },
            PlannedDeparture = new TimeOnly(22, 10),
            PlannedArrival = new TimeOnly(22, 41),
            PlannedPlatform = "4"
        });
        return Task.FromResult(new ResolvedConnection(connection, DateOnly.FromDateTime(DateTime.Today), true));
    }
}
#endif
