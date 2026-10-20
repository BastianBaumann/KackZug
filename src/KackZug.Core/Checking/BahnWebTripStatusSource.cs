using KackZug.Core.Bahn;
using KackZug.Core.Models;

namespace KackZug.Core.Checking;

/// <summary>Prueft eine Teilstrecke ueber Abfahrtstafel und Fahrtdetails von bahn.de, eine Instanz pro Pruefung</summary>
public sealed class BahnWebTripStatusSource : ITripStatusSource
{
    private readonly BahnWebClient _client;
    private readonly Dictionary<string, List<BoardEntry>> _boardCache = new Dictionary<string, List<BoardEntry>>();

    public BahnWebTripStatusSource(BahnWebClient client)
    {
        _client = client;
    }

    public async Task<LegObservation> ObserveAsync(Leg leg, DateOnly travelDate, CancellationToken cancellationToken)
    {
        var plannedDeparture = leg.DepartureOn(travelDate);
        var plannedArrival = leg.ArrivalOn(travelDate);
        if (leg.IsWalk)
        {
            throw new ArgumentException("Fusswege werden nicht abgefragt.", nameof(leg));
        }

        List<BoardEntry> board;
        try
        {
            board = await LoadBoardAsync(leg.From.EvaNumber, plannedDeparture, cancellationToken);
        }
        catch (BahnApiException exception)
        {
            return LegObservation.Unknown(plannedDeparture, plannedArrival, exception.UserMessage);
        }

        if (board.Count == 0)
        {
            return LegObservation.Unknown(plannedDeparture, plannedArrival, "bahn.de hat für diesen Tag und diese Uhrzeit noch keine Daten.");
        }

        var observation = new LegObservation
        {
            PlannedDeparture = plannedDeparture,
            PlannedArrival = plannedArrival
        };

        var entry = BoardMatcher.Find(board, leg, plannedDeparture);
        if (entry is null)
        {
            observation.Result = ObservationResult.NotFound;
            return observation;
        }

        observation.Result = ObservationResult.Found;
        observation.LiveDeparture = entry.LiveTime;
        observation.HasLiveData = entry.LiveTime is not null;
        observation.DepartureCancelled = entry.Cancelled;
        observation.CurrentPlatform = entry.LivePlatform ?? entry.Platform;

        try
        {
            using var trip = await _client.GetTripAsync(entry.JourneyId, cancellationToken);
            TripReader.Apply(trip.RootElement, leg, observation);
        }
        catch (BahnApiException exception)
        {
            observation.Problem = exception.UserMessage;
        }

        return observation;
    }

    private async Task<List<BoardEntry>> LoadBoardAsync(string evaNumber, DateTime plannedDeparture, CancellationToken cancellationToken)
    {
        var from = plannedDeparture.AddMinutes(-1);
        var key = evaNumber + "|" + from.ToString("yyyyMMddHHmm");
        if (_boardCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        using var document = await _client.GetDeparturesAsync(evaNumber, from, cancellationToken);
        var entries = BoardParser.Parse(document.RootElement);
        _boardCache[key] = entries;
        return entries;
    }
}
