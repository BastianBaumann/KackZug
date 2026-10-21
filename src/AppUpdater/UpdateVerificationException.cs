namespace AppUpdater;

/// <summary>Thrown when a downloaded file does not match the expected checksum or size, or no checksum is known.</summary>
public sealed class UpdateVerificationException : Exception
{
    public UpdateVerificationException(string message)
        : base(message)
    {
    }
}
