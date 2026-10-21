namespace AppUpdater;

/// <summary>A feed that knows the newest release of the app, for example GitHub Releases or an own server.</summary>
public interface IUpdateSource
{
    /// <summary>
    /// Returns the release with the highest version, or null if the feed has no release at all.
    /// Throws <see cref="UpdateSourceException"/> when the feed cannot be read or the release has no usable asset.
    /// </summary>
    Task<UpdateRelease?> GetLatestAsync(bool includePrereleases, CancellationToken cancellationToken);
}
