#if DEBUG
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace KackZug.Desktop;

/// <summary>Erfundene GitHub-Antworten fuer die Screenshot-Tour: Release-Liste und APK; ohne Tags antwortet sie wie ein privates Projekt mit 404</summary>
internal sealed class DemoUpdateHandler : HttpMessageHandler
{
    public const string ReleasesUrl = "https://api.github.com/repos/BastianBaumann/KackZug/releases?per_page=20";
    public const string DownloadBaseUrl = "https://github.com/BastianBaumann/KackZug/releases/download/";
    public const string Notes = "## What's Changed\n"
        + "* feat: Updates direkt in der App laden und installieren by @BastianBaumann in https://github.com/BastianBaumann/KackZug/pull/12\n"
        + "* fix: Alarm klingelt auch nach Neustart by @BastianBaumann in https://github.com/BastianBaumann/KackZug/pull/13\n\n"
        + "**Full Changelog**: https://github.com/BastianBaumann/KackZug/compare/v0.1.0...v9.9.9";

    private readonly string? _stableTag;
    private readonly string? _prereleaseTag;

    public DemoUpdateHandler(string? stableTag, string? prereleaseTag = null)
    {
        _stableTag = stableTag;
        _prereleaseTag = prereleaseTag;
    }

    public static byte[] AssetBytes { get; } = CreateAssetBytes();

    public static string AssetSha256
    {
        get { return Convert.ToHexString(SHA256.HashData(AssetBytes)).ToLowerInvariant(); }
    }

    /// <summary>Liefert andere Bytes als angekuendigt, damit die Pruefsumme nicht passt</summary>
    public bool CorruptAsset { get; set; }

    /// <summary>Haelt den Download nach der Haelfte an, bis die Tour weitermacht</summary>
    public DemoPausingStream? PausingStream { get; set; }

    public List<string> RequestedUrls { get; } = new List<string>();

    public int AssetRequests { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri!.AbsoluteUri;
        RequestedUrls.Add(url);
        if (url == ReleasesUrl)
        {
            if (_stableTag is null && _prereleaseTag is null)
            {
                return Task.FromResult(NotFound());
            }

            var releases = new List<string>();
            if (_prereleaseTag is not null)
            {
                releases.Add(ReleaseJson(_prereleaseTag, true));
            }

            if (_stableTag is not null)
            {
                releases.Add(ReleaseJson(_stableTag, false));
            }

            var body = "[" + string.Join(",", releases) + "]";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        }

        if (url.StartsWith(DownloadBaseUrl, StringComparison.Ordinal) && url.EndsWith(".apk", StringComparison.Ordinal))
        {
            AssetRequests++;
            var bytes = AssetBytes;
            if (CorruptAsset)
            {
                bytes = (byte[])AssetBytes.Clone();
                bytes[bytes.Length / 2] ^= 0xFF;
            }

            HttpContent content;
            if (PausingStream is not null)
            {
                PausingStream.SetData(bytes);
                content = new StreamContent(PausingStream);
            }
            else
            {
                content = new ByteArrayContent(bytes);
            }

            content.Headers.ContentLength = bytes.Length;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }

        return Task.FromResult(NotFound());
    }

    private static HttpResponseMessage NotFound()
    {
        return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("{\"message\":\"Not Found\"}") };
    }

    private static string ReleaseJson(string tag, bool prerelease)
    {
        var version = tag.TrimStart('v');
        var assetName = "KackZug-" + version + ".apk";
        var notes = Notes.Replace("\n", "\\n");
        return "{\"tag_name\":\"" + tag + "\",\"name\":\"KackZug " + version + "\",\"draft\":false,\"prerelease\":" + (prerelease ? "true" : "false")
            + ",\"html_url\":\"https://github.com/BastianBaumann/KackZug/releases/tag/" + tag + "\",\"body\":\"" + notes + "\""
            + ",\"assets\":[{\"name\":\"" + assetName + "\",\"size\":" + AssetBytes.Length
            + ",\"browser_download_url\":\"" + DownloadBaseUrl + tag + "/" + assetName + "\",\"digest\":\"sha256:" + AssetSha256 + "\"}]}";
    }

    private static byte[] CreateAssetBytes()
    {
        var bytes = new byte[300_000];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(i * 31 % 251);
        }

        return bytes;
    }
}
#endif
