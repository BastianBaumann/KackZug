namespace KackZug.Core;

/// <summary>Echte Uhrzeit des Geraets</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset Now
    {
        get { return DateTimeOffset.Now; }
    }
}
