using System.Text.RegularExpressions;

namespace KackZug.Core.Import;

/// <summary>Findet im geteilten Text aus dem DB Navigator die Kennung der Verbindung</summary>
public static partial class ShareLinkParser
{
    public static string? FindVbid(string? sharedText)
    {
        if (string.IsNullOrWhiteSpace(sharedText))
        {
            return null;
        }

        var match = BahnLinkPattern().Match(sharedText);
        if (!match.Success)
        {
            return null;
        }

        return match.Groups["vbid"].Value.ToLowerInvariant();
    }

    [GeneratedRegex(@"https?://(?:[\w-]+\.)*bahn\.de/\S*?[?&]vbid=(?<vbid>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})")]
    private static partial Regex BahnLinkPattern();
}
