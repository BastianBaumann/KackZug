using System.Net;
using System.Text.Json;
using KackZug.Core.Models;
using KackZug.Core.Sharing;

namespace KackZug.Core.Tests.Sharing;

/// <summary>Prueft das Wiederfinden der Verbindung und das Erzeugen des Teilen-Links fuer einen Tag</summary>
public class ShareLinkCreatorTests
{
    private static readonly DateOnly Thursday = new DateOnly(2026, 10, 22);

    [Fact]
    public void PicksTheSavedConnectionNotTheEarlierTrain()
    {
        using var document = JsonDocument.Parse(Fixtures.Read("fahrplan.json"));
        var saved = SampleData.CreateShift("Frueh").Outbound!;

        Assert.Equal("recon-passend", ConnectionPicker.PickRecon(document.RootElement, saved));
    }

    [Fact]
    public void PicksNothingWhenTheTrainNumberDiffers()
    {
        using var document = JsonDocument.Parse(Fixtures.Read("fahrplan.json"));
        var saved = SampleData.CreateShift("Frueh").Outbound!;
        saved.Legs[0].TrainNumber = "9999";

        Assert.Null(ConnectionPicker.PickRecon(document.RootElement, saved));
    }

    [Fact]
    public void HandEnteredConnectionWithoutNumberIsFoundByTimes()
    {
        using var document = JsonDocument.Parse(Fixtures.Read("fahrplan.json"));
        var saved = SampleData.CreateShift("Frueh").Outbound!;
        foreach (var leg in saved.Legs)
        {
            leg.TrainNumber = null;
        }

        Assert.Equal("recon-passend", ConnectionPicker.PickRecon(document.RootElement, saved));
    }

    [Fact]
    public async Task SearchesTheDayAndSharesTheMatch()
    {
        var handler = new FakeHttpHandler();
        handler.AnswerWithFixture("angebote/fahrplan", "fahrplan.json");
        handler.Answer("angebote/verbindung/teilen", HttpStatusCode.Created, "{\"vbid\":\"11111111-2222-3333-4444-555555555555\"}");
        var creator = new ShareLinkCreator(handler.CreateBahnClient());

        var link = await creator.CreateAsync(SampleData.CreateShift("Frueh").Outbound!, Thursday, CancellationToken.None);

        Assert.Equal("https://www.bahn.de/buchung/start?vbid=11111111-2222-3333-4444-555555555555", link);
        using var search = JsonDocument.Parse(handler.RequestBodies[0]);
        Assert.Equal("2026-10-22T06:42:00", search.RootElement.GetProperty("anfrageZeitpunkt").GetString());
        Assert.Equal("A=1@O=Musterstadt Hbf@L=8000001@", search.RootElement.GetProperty("abfahrtsHalt").GetString());
        Assert.Equal("A=1@O=Werkstor@L=900002@", search.RootElement.GetProperty("ankunftsHalt").GetString());
        using var share = JsonDocument.Parse(handler.RequestBodies[1]);
        Assert.Equal("recon-passend", share.RootElement.GetProperty("hinfahrtRecon").GetString());
        Assert.Equal("2026-10-22T06:42:00+02:00", share.RootElement.GetProperty("hinfahrtDatum").GetString());
    }

    [Fact]
    public async Task NoLinkWhenTheConnectionIsGone()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("angebote/fahrplan", HttpStatusCode.Created, "{\"verbindungen\":[]}");
        var creator = new ShareLinkCreator(handler.CreateBahnClient());

        var link = await creator.CreateAsync(SampleData.CreateShift("Frueh").Outbound!, Thursday, CancellationToken.None);

        Assert.Null(link);
        Assert.Single(handler.RequestedPaths);
    }

    [Fact]
    public async Task NoLinkWithoutRides()
    {
        var connection = new SavedConnection();
        connection.Legs.Add(new Leg { Kind = LegKind.Walk, Name = "Fussweg" });
        var creator = new ShareLinkCreator(new FakeHttpHandler().CreateBahnClient());

        Assert.Null(await creator.CreateAsync(connection, Thursday, CancellationToken.None));
    }
}
