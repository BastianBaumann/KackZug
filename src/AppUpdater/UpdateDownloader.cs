using System.Security.Cryptography;

namespace AppUpdater;

/// <summary>
/// Downloads the asset of a release into a dedicated directory, checks its SHA-256 while streaming
/// and keeps only the verified file.
/// </summary>
public sealed class UpdateDownloader
{
    public const string PartialSuffix = ".part";

    private const int BufferSize = 81920;

    private readonly HttpClient _httpClient;
    private readonly string? _userAgent;

    public UpdateDownloader(HttpClient httpClient, string? userAgent = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        _userAgent = userAgent;
    }

    /// <summary>When true (default), a release without a known SHA-256 is refused.</summary>
    public bool RequireChecksum { get; set; } = true;

    /// <summary>When true (default), only https download URLs are accepted.</summary>
    public bool RequireHttps { get; set; } = true;

    /// <summary>
    /// Downloads the asset and returns the path of the verified file. Reuses an already verified file.
    /// Removes older update files (same extension) and partial downloads from <paramref name="targetDirectory"/>.
    /// Throws <see cref="UpdateVerificationException"/> on a checksum or size mismatch.
    /// </summary>
    public async Task<string> DownloadAsync(UpdateRelease release, string targetDirectory, IProgress<UpdateProgress>? progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(release);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetDirectory);
        var expectedHash = release.Sha256?.Trim().ToLowerInvariant();
        if (expectedHash is null && RequireChecksum)
        {
            throw new UpdateVerificationException("Release " + release.Tag + " has no SHA-256 checksum");
        }

        var fileName = GetSafeFileName(release.AssetName);
        var uri = new Uri(release.AssetUrl, UriKind.Absolute);
        if (RequireHttps && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new UpdateVerificationException("Download URL is not https: " + release.AssetUrl);
        }

        Directory.CreateDirectory(targetDirectory);
        var finalPath = Path.Combine(targetDirectory, fileName);
        var partialPath = finalPath + PartialSuffix;
        RemoveOtherFiles(targetDirectory, fileName);

        long? total = release.AssetSize > 0 ? release.AssetSize : null;
        if (File.Exists(finalPath))
        {
            if (expectedHash is not null && await IsVerifiedAsync(finalPath, expectedHash, release.AssetSize, cancellationToken))
            {
                var length = new FileInfo(finalPath).Length;
                progress?.Report(new UpdateProgress(length, length));
                return finalPath;
            }

            File.Delete(finalPath);
        }

        string actualHash;
        long received;
        try
        {
            (actualHash, received) = await DownloadToFileAsync(uri, partialPath, total, progress, cancellationToken);
        }
        catch
        {
            TryDelete(partialPath);
            throw;
        }

        if (expectedHash is not null && actualHash != expectedHash)
        {
            TryDelete(partialPath);
            throw new UpdateVerificationException("SHA-256 mismatch for " + fileName + ": expected " + expectedHash + ", got " + actualHash);
        }

        if (release.AssetSize > 0 && received != release.AssetSize)
        {
            TryDelete(partialPath);
            throw new UpdateVerificationException("Size mismatch for " + fileName + ": expected " + release.AssetSize + ", got " + received);
        }

        File.Move(partialPath, finalPath, true);
        return finalPath;
    }

    /// <summary>Computes the SHA-256 of a file as lowercase hex.</summary>
    public static async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private async Task<(string Hash, long Received)> DownloadToFileAsync(Uri uri, string partialPath, long? total, IProgress<UpdateProgress>? progress, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        if (_userAgent is not null)
        {
            request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
        }

        request.Headers.TryAddWithoutValidation("Accept", "application/octet-stream");
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (total is null && response.Content.Headers.ContentLength is long contentLength && contentLength > 0)
        {
            total = contentLength;
        }

        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var output = new FileStream(partialPath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true);
        var buffer = new byte[BufferSize];
        long received = 0;
        progress?.Report(new UpdateProgress(0, total));
        while (true)
        {
            var read = await input.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                break;
            }

            hasher.AppendData(buffer, 0, read);
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            received += read;
            progress?.Report(new UpdateProgress(received, total));
        }

        await output.FlushAsync(cancellationToken);
        var hash = Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant();
        return (hash, received);
    }

    private static async Task<bool> IsVerifiedAsync(string filePath, string expectedHash, long expectedSize, CancellationToken cancellationToken)
    {
        if (expectedSize > 0 && new FileInfo(filePath).Length != expectedSize)
        {
            return false;
        }

        var actual = await ComputeSha256Async(filePath, cancellationToken);
        return actual == expectedHash;
    }

    private static string GetSafeFileName(string assetName)
    {
        var name = Path.GetFileName(assetName);
        if (string.IsNullOrWhiteSpace(name) || name != assetName || name == "." || name == ".."
            || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains('/') || name.Contains('\\'))
        {
            throw new UpdateVerificationException("Unsafe asset file name: " + assetName);
        }

        return name;
    }

    private static void RemoveOtherFiles(string directory, string keepFileName)
    {
        var extension = Path.GetExtension(keepFileName);
        foreach (var path in Directory.EnumerateFiles(directory))
        {
            var name = Path.GetFileName(path);
            if (string.Equals(name, keepFileName, StringComparison.Ordinal))
            {
                continue;
            }

            var isPartial = name.EndsWith(PartialSuffix, StringComparison.OrdinalIgnoreCase);
            var isOldUpdate = extension.Length > 0 && string.Equals(Path.GetExtension(name), extension, StringComparison.OrdinalIgnoreCase);
            if (isPartial || isOldUpdate)
            {
                TryDelete(path);
            }
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
