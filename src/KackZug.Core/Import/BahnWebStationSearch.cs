using KackZug.Core.Bahn;
using KackZug.Core.Models;

namespace KackZug.Core.Import;

/// <summary>Bahnhofssuche ueber bahn.de</summary>
public sealed class BahnWebStationSearch : IStationSearch
{
    private readonly BahnWebClient _client;

    public BahnWebStationSearch(BahnWebClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<Station>> SearchAsync(string term, CancellationToken cancellationToken)
    {
        using var document = await _client.FindLocationsAsync(term, cancellationToken);
        return StationSearchParser.Parse(document.RootElement);
    }
}
