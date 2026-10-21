using System.Net;
using System.Text.Json;

namespace AppUpdater;

/// <summary>
/// Reads releases of a public GitHub repository and returns the one with the highest version
/// that carries an asset matching the given pattern.
/// </summary>
public sealed class GitHubReleaseSource : IUpdateSource
{
    public const string ApiBaseAddress = "https://api.github.com";
    public const int PageSize = 20;
    public const string ChecksumSuffix = ".sha256";

    private const long MaxChecksumFileBytes = 4096;

    private readonly HttpClient _httpClient;
    private readonly string _owner;
    private readonly string _repo;
    private readonly string _assetPattern;
    private readonly string _userAgent;

    /// <param name="httpClient">Client used for the API and the checksum files.</param>
    /// <param name="owner">Repository owner, for example "octocat".</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="assetPattern">Wildcard pattern for the asset to install, for example "*.apk".</param>
    /// <param name="userAgent">User-Agent header; GitHub rejects requests without one, for example "MyApp/1.2.3".</param>
    public GitHubReleaseSource(HttpClient httpClient, string owner, string repo, string assetPattern, string userAgent)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(repo);
        ArgumentException.ThrowIfNullOrWhiteSpace(assetPattern);
        ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);
        _httpClient = httpClient;
        _owner = owner;
        _repo = repo;
        _assetPattern = assetPattern;
        _userAgent = userAgent;
    }

    public string ReleasesUrl
    {
        get
        {
            return ApiBaseAddress + "/repos/" + Uri.EscapeDataString(_owner) + "/" + Uri.EscapeDataString(_repo)
                + "/releases?per_page=" + PageSize;
        }
    }

    public async Task<UpdateRelease?> GetLatestAsync(bool includePrereleases, CancellationToken cancellationToken)
    {
        var text = await GetStringAsync(ReleasesUrl, true, cancellationToken);
        var best = FindBestRelease(text, includePrereleases);
        if (best is null)
        {
            return null;
        }

        var (version, release) = best.Value;
        var asset = FindAsset(release);
        if (asset is null)
        {
            throw new UpdateSourceException(UpdateFailureReason.NoMatchingAsset,
                "Release " + GetString(release, "tag_name") + " has no asset matching '" + _assetPattern + "'");
        }

        var assetName = GetString(asset.Value, "name")!;
        var assetUrl = GetString(asset.Value, "browser_download_url");
        if (string.IsNullOrEmpty(assetUrl))
        {
            throw new UpdateSourceException(UpdateFailureReason.InvalidResponse, "Asset " + assetName + " has no download URL");
        }

        var sha256 = ParseDigest(GetString(asset.Value, "digest"));
        if (sha256 is null)
        {
            sha256 = await ReadChecksumFileAsync(release, assetName, cancellationToken);
        }

        return new UpdateRelease
        {
            Version = version,
            Tag = GetString(release, "tag_name")!,
            Title = GetString(release, "name") ?? string.Empty,
            Notes = GetString(release, "body") ?? string.Empty,
            IsPrerelease = GetBool(release, "prerelease") || version.IsPrerelease,
            PageUrl = GetString(release, "html_url") ?? string.Empty,
            AssetName = assetName,
            AssetUrl = assetUrl,
            AssetSize = GetLong(asset.Value, "size"),
            Sha256 = sha256
        };
    }

    /// <summary>Returns the lowercase hex from a GitHub digest such as "sha256:abc...", or null if it is not a SHA-256 digest.</summary>
    public static string? ParseDigest(string? digest)
    {
        const string prefix = "sha256:";
        if (digest is null || !digest.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return NormalizeHex(digest.Substring(prefix.Length));
    }

    /// <summary>Returns the first token of a checksum file (as written by sha256sum) as lowercase hex, or null.</summary>
    public static string? ParseChecksumFile(string content)
    {
        var tokens = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return null;
        }

        return NormalizeHex(tokens[0]);
    }

    private static string? NormalizeHex(string text)
    {
        var hex = text.Trim().ToLowerInvariant();
        if (hex.Length != 64 || !hex.All(char.IsAsciiHexDigit))
        {
            return null;
        }

        return hex;
    }

    private (AppVersion Version, JsonElement Release)? FindBestRelease(string json, bool includePrereleases)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new UpdateSourceException(UpdateFailureReason.InvalidResponse, "Release list is not a JSON array");
            }

            AppVersion? bestVersion = null;
            JsonElement bestRelease = default;
            foreach (var release in document.RootElement.EnumerateArray())
            {
                if (release.ValueKind != JsonValueKind.Object || GetBool(release, "draft"))
                {
                    continue;
                }

                if (!AppVersion.TryParse(GetString(release, "tag_name"), out var version))
                {
                    continue;
                }

                var isPrerelease = GetBool(release, "prerelease") || version.IsPrerelease;
                if (isPrerelease && !includePrereleases)
                {
                    continue;
                }

                if (bestVersion is null || version > bestVersion)
                {
                    bestVersion = version;
                    bestRelease = release.Clone();
                }
            }

            if (bestVersion is null)
            {
                return null;
            }

            return (bestVersion, bestRelease);
        }
        catch (JsonException exception)
        {
            throw new UpdateSourceException(UpdateFailureReason.InvalidResponse, "Release list is not valid JSON", exception);
        }
    }

    private JsonElement? FindAsset(JsonElement release)
    {
        if (!release.TryGetProperty("assets", out var assets) || assets.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var asset in assets.EnumerateArray())
        {
            var name = GetString(asset, "name");
            if (name is null || name.EndsWith(ChecksumSuffix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (WildcardPattern.IsMatch(name, _assetPattern))
            {
                return asset;
            }
        }

        return null;
    }

    private async Task<string?> ReadChecksumFileAsync(JsonElement release, string assetName, CancellationToken cancellationToken)
    {
        var checksumName = assetName + ChecksumSuffix;
        foreach (var asset in release.GetProperty("assets").EnumerateArray())
        {
            if (!string.Equals(GetString(asset, "name"), checksumName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var url = GetString(asset, "browser_download_url");
            if (string.IsNullOrEmpty(url) || GetLong(asset, "size") > MaxChecksumFileBytes)
            {
                return null;
            }

            var content = await GetStringAsync(url, false, cancellationToken);
            return ParseChecksumFile(content);
        }

        return null;
    }

    private async Task<string> GetStringAsync(string url, bool isApi, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
            if (isApi)
            {
                request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github+json");
                request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", "2022-11-28");
            }

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new UpdateSourceException(UpdateFailureReason.RateLimited, "GitHub answered " + (int)response.StatusCode + " for " + url);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UpdateSourceException(UpdateFailureReason.InvalidResponse, "GitHub answered 404 for " + url);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new UpdateSourceException(UpdateFailureReason.Network, "GitHub answered " + (int)response.StatusCode + " for " + url);
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new UpdateSourceException(UpdateFailureReason.Network, exception.Message, exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new UpdateSourceException(UpdateFailureReason.Network, "Request timed out: " + url, exception);
        }
    }

    private static string? GetString(JsonElement element, string name)
    {
        if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    private static bool GetBool(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
    }

    private static long GetLong(JsonElement element, string name)
    {
        if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number))
        {
            return number;
        }

        return 0;
    }
}
