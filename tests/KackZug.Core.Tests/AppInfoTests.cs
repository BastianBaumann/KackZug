using KackZug.Core;

namespace KackZug.Core.Tests;

/// <summary>Prueft die festen App-Angaben</summary>
public class AppInfoTests
{
    [Fact]
    public void VersionHasThreeParts()
    {
        var parts = AppInfo.Version.Split('.');

        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void FullVersionStartsWithVersion()
    {
        Assert.StartsWith(AppInfo.Version, AppInfo.FullVersion);
        Assert.DoesNotContain("+", AppInfo.FullVersion);
    }

    [Theory]
    [InlineData("0.3.0-dev.2+abc123", "0.3.0-dev.2")]
    [InlineData("1.2.0", "1.2.0")]
    [InlineData("1.2.0+5f1e", "1.2.0")]
    [InlineData(null, "9.9.9")]
    [InlineData("", "9.9.9")]
    [InlineData("+abc", "9.9.9")]
    [InlineData("unbekannt", "9.9.9")]
    public void CleanFullVersionDropsBuildMetadata(string? informational, string expected)
    {
        Assert.Equal(expected, AppInfo.CleanFullVersion(informational, "9.9.9"));
    }
}
