namespace KackZug.Core.Tests;

/// <summary>Uhr mit fester Zeit fuer Tests</summary>
public sealed class FixedClock : IClock
{
    public FixedClock(DateTimeOffset now)
    {
        Now = now;
    }

    public DateTimeOffset Now { get; set; }
}
