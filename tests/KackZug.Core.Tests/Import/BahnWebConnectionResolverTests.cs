using System.Net;
using KackZug.Core.Bahn;
using KackZug.Core.Import;
using KackZug.Core.Models;

namespace KackZug.Core.Tests.Import;

/// <summary>Prueft den Import einer geteilten Verbindung mit erfundenen Antworten</summary>
public class BahnWebConnectionResolverTests
{
    private const string SharedText = "Fahrt ansehen: https://int.bahn.de/en/buchung/start?vbid=11111111-2222-3333-4444-555555555555";
    private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.FromHours(2));

    [Fact]
    public async Task MapsTrainWalkAndTram()
    {
        var handler = new FakeHttpHandler();
        handler.AnswerWithFixture("angebote/verbindung/", "verbindung.json");
        handler.AnswerWithFixture("angebote/recon", "recon-transfer.json");
        var resolver = CreateResolver(handler);

        var result = await resolver.ResolveAsync(SharedText, CancellationToken.None);

        Assert.Equal(new DateOnly(2026, 10, 20), result.TravelDate);
        Assert.True(result.HasPlatforms);
        var legs = result.Connection.Legs;
        Assert.Equal(3, legs.Count);

        var train = legs[0];
        Assert.Equal(LegKind.PublicTransport, train.Kind);
        Assert.Equal("RE7", train.Name);
        Assert.Equal("NRE", train.Category);
        Assert.Equal("REGIONAL", train.ProductClass);
        Assert.Equal("4711", train.TrainNumber);
        Assert.Equal("Musterstadt Hbf", train.From.Name);
        Assert.Equal("8000001", train.From.EvaNumber);
        Assert.Equal("Beispielhausen", train.To.Name);
        Assert.Equal(new TimeOnly(6, 42), train.PlannedDeparture);
        Assert.Equal(new TimeOnly(7, 10), train.PlannedArrival);
        Assert.Equal("2", train.PlannedPlatform);

        Assert.Equal(LegKind.Walk, legs[1].Kind);
        Assert.Equal(new TimeOnly(7, 15), legs[1].PlannedArrival);

        var tram = legs[2];
        Assert.Equal("STR 2", tram.Name);
        Assert.Equal("2", tram.Line);
        Assert.Null(tram.PlannedPlatform);

        Assert.Equal(2, handler.RequestedPaths.Count);
        Assert.Contains("\"ctxRecon\":\"", handler.RequestBodies[0]);
    }

    [Fact]
    public async Task ConnectionOverMidnightGetsDayOffsets()
    {
        var handler = new FakeHttpHandler();
        handler.AnswerWithFixture("angebote/verbindung/", "verbindung.json");
        handler.AnswerWithFixture("angebote/recon", "recon-midnight.json");
        var resolver = CreateResolver(handler);

        var result = await resolver.ResolveAsync(SharedText, CancellationToken.None);

        var legs = result.Connection.Legs;
        Assert.Equal(0, legs[0].DepartureDayOffset);
        Assert.Equal(1, legs[0].ArrivalDayOffset);
        Assert.Equal(1, legs[1].DepartureDayOffset);
    }

    [Fact]
    public async Task FallsBackToReconStringWhenDetailsAreBlocked()
    {
        var handler = new FakeHttpHandler();
        handler.AnswerWithFixture("angebote/verbindung/", "verbindung.json");
        handler.Answer("angebote/recon", HttpStatusCode.Forbidden, "{\"status\":\"ERROR\",\"code\":\"OPS_BLOCKED\"}");
        var resolver = CreateResolver(handler);

        var result = await resolver.ResolveAsync(SharedText, CancellationToken.None);

        Assert.False(result.HasPlatforms);
        var legs = result.Connection.Legs;
        Assert.Equal(3, legs.Count);
        Assert.Equal("Musterstadt Hbf", legs[0].From.Name);
        Assert.Equal("8000001", legs[0].From.EvaNumber);
        Assert.Equal("4711", legs[0].TrainNumber);
        Assert.Equal(new TimeOnly(6, 42), legs[0].PlannedDeparture);
        Assert.Equal(LegKind.Walk, legs[1].Kind);
        Assert.Equal("STR 2", legs[2].Name);
    }

    [Fact]
    public async Task UnknownLinkGivesClearMessage()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("angebote/verbindung/", HttpStatusCode.InternalServerError, "{}");
        var resolver = CreateResolver(handler);

        var exception = await Assert.ThrowsAsync<ImportException>(() => resolver.ResolveAsync(SharedText, CancellationToken.None));

        Assert.Contains("neu teilen", exception.Message);
    }

    [Fact]
    public async Task TextWithoutLinkIsRejectedWithoutRequest()
    {
        var handler = new FakeHttpHandler();
        var resolver = CreateResolver(handler);

        await Assert.ThrowsAsync<ImportException>(() => resolver.ResolveAsync("nur Text", CancellationToken.None));

        Assert.Empty(handler.RequestedPaths);
    }

    private static BahnWebConnectionResolver CreateResolver(FakeHttpHandler handler)
    {
        var client = new BahnWebClient(handler.CreateClient());
        return new BahnWebConnectionResolver(client, new FixedClock(Now));
    }
}
