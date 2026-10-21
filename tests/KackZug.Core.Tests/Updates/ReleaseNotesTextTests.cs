using KackZug.Core.Updates;

namespace KackZug.Core.Tests.Updates;

/// <summary>Prueft das Kuerzen der Release-Notizen fuer den Update-Hinweis</summary>
public class ReleaseNotesTextTests
{
    [Fact]
    public void GeneratedNotesBecomePlainLines()
    {
        var notes = "## What's Changed\r\n* feat: neue Woche by @BastianBaumann in https://github.com/BastianBaumann/KackZug/pull/7\r\n"
            + "* fix: **Alarm** bleibt an by @BastianBaumann in https://github.com/BastianBaumann/KackZug/pull/8\r\n\r\n"
            + "**Full Changelog**: https://github.com/BastianBaumann/KackZug/compare/v0.1.0...v0.2.0";

        var text = ReleaseNotesText.Shorten(notes, 3);

        Assert.Equal("feat: neue Woche\nfix: Alarm bleibt an", text);
    }

    [Fact]
    public void LongNotesAreCutWithEllipsis()
    {
        var notes = "- eins\n- zwei\n- drei\n- vier";

        var text = ReleaseNotesText.Shorten(notes, 3);

        Assert.Equal("eins\nzwei\ndrei …", text);
    }

    [Fact]
    public void ExactlyMaxLinesHasNoEllipsis()
    {
        Assert.Equal("eins\nzwei", ReleaseNotesText.Shorten("eins\n\nzwei\n", 2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("## What's Changed\n\n**Full Changelog**: https://example.org")]
    public void EmptyOrOnlyBoilerplateGivesEmptyText(string? notes)
    {
        Assert.Equal("", ReleaseNotesText.Shorten(notes, 3));
    }

    [Fact]
    public void NewContributorsSectionIsSkipped()
    {
        var notes = "* feat: x\n## New Contributors\n* @someone made their first contribution in https://example.org/pull/1";

        Assert.Equal("feat: x", ReleaseNotesText.Shorten(notes, 3));
    }
}
