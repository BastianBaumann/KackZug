using System.Text.Json;
using KackZug.Core.Bahn;

namespace KackZug.Core.Updates;

/// <summary>Schaut hoechstens einmal am Tag auf GitHub nach, ob es eine neuere Version gibt</summary>
public sealed class UpdateChecker
{
    public const string LatestReleaseUrl = "https://api.github.com/repos/BastianBaumann/KackZug/releases/latest";

    private readonly HttpClient _httpClient;

    public UpdateChecker(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public static bool IsDue(DateTimeOffset? lastCheck, DateTimeOffset now)
    {
        if (lastCheck is null)
        {
            return true;
        }

        var lastDay = DateOnly.FromDateTime(lastCheck.Value.LocalDateTime);
        var today = DateOnly.FromDateTime(now.LocalDateTime);
        return lastDay != today;
    }

    public async Task<UpdateInfo?> FindNewerAsync(string currentVersion, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, LatestReleaseUrl);
            request.Headers.TryAddWithoutValidation("User-Agent", "KackZug/" + currentVersion);
            request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github+json");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(text);
            var tag = BahnJson.GetString(document.RootElement, "tag_name");
            var pageUrl = BahnJson.GetString(document.RootElement, "html_url");
            if (tag is null || pageUrl is null)
            {
                return null;
            }

            if (!IsNewer(tag, currentVersion))
            {
                return null;
            }

            return new UpdateInfo(CleanVersion(tag), pageUrl);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static bool IsNewer(string releaseTag, string currentVersion)
    {
        var release = ParseVersion(releaseTag);
        var current = ParseVersion(currentVersion);
        if (release is null || current is null)
        {
            return false;
        }

        return release > current;
    }

    private static Version? ParseVersion(string text)
    {
        var cleaned = CleanVersion(text);
        var parsed = Version.TryParse(cleaned, out var version);
        if (!parsed)
        {
            return null;
        }

        return version;
    }

    private static string CleanVersion(string text)
    {
        var cleaned = text.Trim();
        if (cleaned.StartsWith('v') || cleaned.StartsWith('V'))
        {
            cleaned = cleaned.Substring(1);
        }

        var suffixStart = cleaned.IndexOfAny(new[] { '-', '+' });
        if (suffixStart >= 0)
        {
            cleaned = cleaned.Substring(0, suffixStart);
        }

        return cleaned;
    }
}
