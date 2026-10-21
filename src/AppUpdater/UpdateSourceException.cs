namespace AppUpdater;

/// <summary>Thrown by an <see cref="IUpdateSource"/> when the feed cannot be read or has no usable asset.</summary>
public sealed class UpdateSourceException : Exception
{
    public UpdateSourceException(UpdateFailureReason reason, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Reason = reason;
    }

    public UpdateFailureReason Reason { get; }
}
