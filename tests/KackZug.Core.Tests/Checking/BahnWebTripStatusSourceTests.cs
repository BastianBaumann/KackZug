using System.Net;
using System.Text.Json.Nodes;
using KackZug.Core.Checking;
using KackZug.Core.Models;

namespace KackZug.Core.Tests.Checking;

/// <summary>Prueft das Abfragen einer Teilstrecke mit erfundenen Antworten</summary>
public class BahnWebTripStatusSourceTests
{
    private static readonly DateOnly TravelDate = new DateOnly(2026, 10, 21);

    [Fact]
    public async Task FindsDelayedTrainWithNewPlatform()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("reiseloesung/abfahrten", HttpStatusCode.OK, BahnJsonBuilder.Board(
            BahnJsonBuilder.BoardEntry("2026-10-21T06:42:00", "Bus 5", "Bus", "5", "1"),
            WithLiveTime(BahnJsonBuilder.BoardEntry("2026-10-21T06:42:00", "RE7", "NRE", "4711", "2"), "2026-10-21T06:50:00")));
        handler.Answer("reiseloesung/fahrt", HttpStatusCode.OK, BahnJsonBuilder.Trip(false,
            WithChangedPlatform(BahnJsonBuilder.Stop("8000001", "Musterstadt Hbf", null, null, "2026-10-21T06:42:00", "2026-10-21T06:50:00", "2"), "3"),
            BahnJsonBuilder.Stop("8000002", "Beispielhausen", "2026-10-21T07:10:00", "2026-10-21T07:18:00", null, null, "5")));

        var observation = await CreateSource(handler).ObserveAsync(TrainLeg(), TravelDate, CancellationToken.None);

        Assert.Equal(ObservationResult.Found, observation.Result);
        Assert.Equal(new DateTime(2026, 10, 21, 6, 50, 0), observation.LiveDeparture);
        Assert.Equal(new DateTime(2026, 10, 21, 7, 18, 0), observation.LiveArrival);
        Assert.True(observation.ArrivalKnown);
        Assert.Equal("3", observation.CurrentPlatform);
        Assert.False(observation.DepartureCancelled);
        Assert.Contains("ortExtId=8000001", handler.RequestedPaths[0]);
        Assert.Contains("zeit=06%3A41%3A00", handler.RequestedPaths[0]);
        Assert.Contains("datum=2026-10-21", handler.RequestedPaths[0]);
        Assert.True(observation.HasLiveData);
    }

    [Fact]
    public async Task CancelledTripIsReported()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("reiseloesung/abfahrten", HttpStatusCode.OK, BahnJsonBuilder.Board(
            BahnJsonBuilder.BoardEntry("2026-10-21T06:42:00", "RE7", "NRE", "4711", "2")));
        handler.Answer("reiseloesung/fahrt", HttpStatusCode.OK, BahnJsonBuilder.Trip(true,
            BahnJsonBuilder.Stop("8000001", "Musterstadt Hbf", null, null, "2026-10-21T06:42:00", null, "2"),
            BahnJsonBuilder.Stop("8000002", "Beispielhausen", "2026-10-21T07:10:00", null, null, null, "5")));

        var observation = await CreateSource(handler).ObserveAsync(TrainLeg(), TravelDate, CancellationToken.None);

        Assert.True(observation.DepartureCancelled);
    }

    [Fact]
    public async Task CancelledNoteOnBoardIsReported()
    {
        var entry = BahnJsonBuilder.BoardEntry("2026-10-21T06:42:00", "RE7", "NRE", "4711", "2");
        entry["meldungen"] = new JsonArray(new JsonObject { ["type"] = "HALT_AUSFALL", ["text"] = "Halt entfällt" });
        var handler = new FakeHttpHandler();
        handler.Answer("reiseloesung/abfahrten", HttpStatusCode.OK, BahnJsonBuilder.Board(entry));
        handler.Answer("reiseloesung/fahrt", HttpStatusCode.OK, BahnJsonBuilder.Trip(false));

        var observation = await CreateSource(handler).ObserveAsync(TrainLeg(), TravelDate, CancellationToken.None);

        Assert.True(observation.DepartureCancelled);
    }

    [Fact]
    public async Task MissingDestinationStopMeansPartlyCancelled()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("reiseloesung/abfahrten", HttpStatusCode.OK, BahnJsonBuilder.Board(
            BahnJsonBuilder.BoardEntry("2026-10-21T06:42:00", "RE7", "NRE", "4711", "2")));
        handler.Answer("reiseloesung/fahrt", HttpStatusCode.OK, BahnJsonBuilder.Trip(false,
            BahnJsonBuilder.Stop("8000001", "Musterstadt Hbf", null, null, "2026-10-21T06:42:00", null, "2"),
            BahnJsonBuilder.Stop("8000009", "Zwischenort", "2026-10-21T06:55:00", null, null, null, "1")));

        var observation = await CreateSource(handler).ObserveAsync(TrainLeg(), TravelDate, CancellationToken.None);

        Assert.True(observation.ArrivalCancelled);
        Assert.False(observation.DepartureCancelled);
    }

    [Fact]
    public async Task OtherTrainAtSameTimeIsNotTaken()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("reiseloesung/abfahrten", HttpStatusCode.OK, BahnJsonBuilder.Board(
            BahnJsonBuilder.BoardEntry("2026-10-21T06:42:00", "Bus 5", "Bus", "5", "1"),
            BahnJsonBuilder.BoardEntry("2026-10-21T06:43:00", "RE7", "NRE", "4711", "2")));

        var observation = await CreateSource(handler).ObserveAsync(TrainLeg(), TravelDate, CancellationToken.None);

        Assert.Equal(ObservationResult.NotFound, observation.Result);
        Assert.Single(handler.RequestedPaths);
    }

    [Fact]
    public async Task PlannedDataWithoutRealtimeIsMarked()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("reiseloesung/abfahrten", HttpStatusCode.OK, BahnJsonBuilder.Board(
            BahnJsonBuilder.BoardEntry("2026-10-21T06:42:00", "RE7", "NRE", "4711", "2")));
        handler.Answer("reiseloesung/fahrt", HttpStatusCode.OK, BahnJsonBuilder.Trip(false,
            BahnJsonBuilder.Stop("8000001", "Musterstadt Hbf", null, null, "2026-10-21T06:42:00", null, "2"),
            BahnJsonBuilder.Stop("8000002", "Beispielhausen", "2026-10-21T07:10:00", null, null, null, "5")));

        var observation = await CreateSource(handler).ObserveAsync(TrainLeg(), TravelDate, CancellationToken.None);

        Assert.Equal(ObservationResult.Found, observation.Result);
        Assert.False(observation.HasLiveData);
    }

    [Fact]
    public async Task EmptyBoardMeansNotYetKnownInsteadOfNotFound()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("reiseloesung/abfahrten", HttpStatusCode.OK, BahnJsonBuilder.Board());

        var observation = await CreateSource(handler).ObserveAsync(TrainLeg(), TravelDate, CancellationToken.None);

        Assert.Equal(ObservationResult.Unknown, observation.Result);
        Assert.Contains("noch keine Daten", observation.Problem);
    }

    [Fact]
    public async Task BlockedBoardMeansUnknown()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("reiseloesung/abfahrten", HttpStatusCode.Forbidden, "Access Denied");

        var observation = await CreateSource(handler).ObserveAsync(TrainLeg(), TravelDate, CancellationToken.None);

        Assert.Equal(ObservationResult.Unknown, observation.Result);
        Assert.NotNull(observation.Problem);
    }

    [Fact]
    public async Task BlockedTripDetailsKeepBoardData()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("reiseloesung/abfahrten", HttpStatusCode.OK, BahnJsonBuilder.Board(
            WithLiveTime(BahnJsonBuilder.BoardEntry("2026-10-21T06:42:00", "RE7", "NRE", "4711", "2"), "2026-10-21T06:47:00")));
        handler.Answer("reiseloesung/fahrt", HttpStatusCode.OK, "{\"status\":\"ERROR\",\"code\":\"OPS_BLOCKED\"}");

        var observation = await CreateSource(handler).ObserveAsync(TrainLeg(), TravelDate, CancellationToken.None);

        Assert.Equal(ObservationResult.Found, observation.Result);
        Assert.Equal(new DateTime(2026, 10, 21, 6, 47, 0), observation.LiveDeparture);
        Assert.False(observation.ArrivalKnown);
        Assert.NotNull(observation.Problem);
    }

    [Fact]
    public async Task SameBoardIsLoadedOnlyOnce()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("reiseloesung/abfahrten", HttpStatusCode.OK, BahnJsonBuilder.Board());
        var source = CreateSource(handler);

        await source.ObserveAsync(TrainLeg(), TravelDate, CancellationToken.None);
        await source.ObserveAsync(TrainLeg(), TravelDate, CancellationToken.None);

        Assert.Single(handler.RequestedPaths);
    }

    private static BahnWebTripStatusSource CreateSource(FakeHttpHandler handler)
    {
        return new BahnWebTripStatusSource(handler.CreateBahnClient());
    }

    private static Leg TrainLeg()
    {
        return SampleData.CreateShift("Fruehdienst").Outbound!.Legs[0];
    }

    private static JsonObject WithLiveTime(JsonObject entry, string liveTime)
    {
        entry["ezZeit"] = liveTime;
        return entry;
    }

    private static JsonObject WithChangedPlatform(JsonObject stop, string platform)
    {
        stop["ezGleis"] = platform;
        return stop;
    }
}
