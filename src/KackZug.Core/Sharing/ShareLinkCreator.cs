using KackZug.Core.Bahn;
using KackZug.Core.Models;
using KackZug.Core.Planning;

namespace KackZug.Core.Sharing;

/// <summary>Erzeugt bei bahn.de einen Teilen-Link fuer die gespeicherte Verbindung an einem bestimmten Tag</summary>
/// <remarks>Den Link oeffnet der DB Navigator direkt bei der Verbindung, eine Suchadresse nimmt er nicht an.</remarks>
public sealed class ShareLinkCreator : IShareLinkCreator
{
    public const string ShareBase = "https://www.bahn.de/buchung/start?vbid=";

    private readonly BahnWebClient _client;

    public ShareLinkCreator(BahnWebClient client)
    {
        _client = client;
    }

    public async Task<string?> CreateAsync(SavedConnection connection, DateOnly travelDate, CancellationToken cancellationToken)
    {
        var firstRide = connection.FirstRide;
        var lastRide = connection.LastRide;
        if (firstRide is null || lastRide is null)
        {
            return null;
        }

        var departure = firstRide.DepartureOn(travelDate);
        using var document = await _client.SearchConnectionsAsync(
            StationIds.For(firstRide.From), StationIds.For(lastRide.To), departure, cancellationToken);
        var recon = ConnectionPicker.PickRecon(document.RootElement, connection);
        if (recon is null)
        {
            return null;
        }

        var instant = BerlinTime.ToInstant(departure, BerlinTime.Zone);
        var vbid = await _client.CreateShareAsync(firstRide.From.Name, lastRide.To.Name, instant, recon, cancellationToken);
        return ShareBase + vbid;
    }
}
