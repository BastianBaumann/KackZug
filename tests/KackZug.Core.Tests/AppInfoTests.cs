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
}
