namespace AppUpdater;

/// <summary>Download progress: bytes received so far and the total, if known.</summary>
public readonly record struct UpdateProgress(long BytesReceived, long? TotalBytes)
{
    /// <summary>Progress between 0 and 1, or null if the total size is unknown.</summary>
    public double? Fraction
    {
        get
        {
            if (TotalBytes is null || TotalBytes.Value <= 0)
            {
                return null;
            }

            return Math.Min(1.0, (double)BytesReceived / TotalBytes.Value);
        }
    }
}
