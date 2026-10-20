using KackZug.Core.Import;

namespace KackZug.Core.Tests.Import;

/// <summary>Prueft das Finden des Links im geteilten Text</summary>
public class ShareLinkParserTests
{
    [Fact]
    public void FindsVbidInEnglishShareText()
    {
        var text = "Musterstadt Hbf → Werkstor\nTue 20.10.2026\n\nRE7 (4711)\n\nView journey: https://int.bahn.de/en/buchung/start?vbid=3847A47E-14ab-47e9-a125-26c80ba7bb5b";

        var vbid = ShareLinkParser.FindVbid(text);

        Assert.Equal("3847a47e-14ab-47e9-a125-26c80ba7bb5b", vbid);
    }

    [Fact]
    public void FindsVbidOnWwwHostWithMoreParameters()
    {
        var text = "Verbindung ansehen: https://www.bahn.de/buchung/start?lang=de&vbid=c43894cb-6ff9-47dd-b46a-b3271c7bcfa4 Gute Fahrt";

        var vbid = ShareLinkParser.FindVbid(text);

        Assert.Equal("c43894cb-6ff9-47dd-b46a-b3271c7bcfa4", vbid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Hallo, wie geht es?")]
    [InlineData("https://example.com/buchung/start?vbid=c43894cb-6ff9-47dd-b46a-b3271c7bcfa4")]
    [InlineData("https://www.bahn.de/buchung/start?vbid=kaputt")]
    public void ReturnsNullWithoutBahnLink(string? text)
    {
        Assert.Null(ShareLinkParser.FindVbid(text));
    }
}
