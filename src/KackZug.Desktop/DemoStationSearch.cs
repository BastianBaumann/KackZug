#if DEBUG
using KackZug.Core.Import;
using KackZug.Core.Models;

namespace KackZug.Desktop;

/// <summary>Erfundene Bahnhofssuche fuer die Screenshot-Tour, damit keine Anfragen an bahn.de gehen</summary>
internal sealed class DemoStationSearch : IStationSearch
{
    private static readonly Station[] Stations =
    {
        new Station { Name = "Musterstadt Hbf", EvaNumber = "8000001" },
        new Station { Name = "Musterstadt Nord", EvaNumber = "8000003" },
        new Station { Name = "Beispielhausen", EvaNumber = "8000002" },
        new Station { Name = "Beispielhausen Markt", EvaNumber = "900001" },
        new Station { Name = "Werkstor, Beispielhausen", EvaNumber = "900002" }
    };

    public Task<IReadOnlyList<Station>> SearchAsync(string term, CancellationToken cancellationToken)
    {
        IReadOnlyList<Station> found = Stations.Where(station => station.Name.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult(found);
    }
}
#endif
