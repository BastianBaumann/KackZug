using System.Net;
using KackZug.Core.Updates;

namespace KackZug.Core.Tests.Updates;

/// <summary>Prueft den Versionsvergleich und die Regel einmal pro Tag</summary>
public class UpdateCheckerTests
{
    [Theory]
    [InlineData("v1.2.0", "1.1.9", true)]
    [InlineData("v1.10.0", "1.9.0", true)]
    [InlineData("v1.2.0", "1.2.0", false)]
    [InlineData("1.1.0", "1.2.0", false)]
    [InlineData("v2.0.0-beta", "1.9.0", true)]
    [InlineData("kaputt", "1.0.0", false)]
    public void ComparesVersions(string tag, string current, bool expected)
    {
        Assert.Equal(expected, UpdateChecker.IsNewer(tag, current));
    }

    [Fact]
    public void ChecksOnlyOncePerDay()
    {
        var morning = new DateTimeOffset(2026, 10, 21, 7, 0, 0, TimeSpan.FromHours(2)).ToLocalTime();

        Assert.True(UpdateChecker.IsDue(null, morning));
        Assert.False(UpdateChecker.IsDue(morning, morning.AddHours(3)));
        Assert.True(UpdateChecker.IsDue(morning, morning.AddDays(1)));
    }

    [Fact]
    public async Task ReportsNewerRelease()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("/repos/BastianBaumann/KackZug/releases/latest", HttpStatusCode.OK,
            "{\"tag_name\":\"v0.2.0\",\"html_url\":\"https://github.com/BastianBaumann/KackZug/releases/tag/v0.2.0\"}");
        var checker = new UpdateChecker(handler.CreateClient());

        var update = await checker.FindNewerAsync("0.1.0", CancellationToken.None);

        Assert.NotNull(update);
        Assert.Equal("0.2.0", update.Version);
        Assert.EndsWith("/releases/tag/v0.2.0", update.PageUrl);
    }

    [Fact]
    public async Task MissingReleaseIsQuiet()
    {
        var handler = new FakeHttpHandler();
        handler.Answer("/repos/BastianBaumann/KackZug/releases/latest", HttpStatusCode.NotFound, "{\"message\":\"Not Found\"}");
        var checker = new UpdateChecker(handler.CreateClient());

        var update = await checker.FindNewerAsync("0.1.0", CancellationToken.None);

        Assert.Null(update);
    }

    [Fact]
    public async Task OfflineIsQuiet()
    {
        var handler = new FakeHttpHandler();
        var checker = new UpdateChecker(handler.CreateClient());

        var update = await checker.FindNewerAsync("0.1.0", CancellationToken.None);

        Assert.Null(update);
    }
}
