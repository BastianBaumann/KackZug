namespace KackZug.Core;

/// <summary>Liefert die aktuelle Zeit, damit Tests eine feste Zeit vorgeben koennen</summary>
public interface IClock
{
    DateTimeOffset Now { get; }
}
