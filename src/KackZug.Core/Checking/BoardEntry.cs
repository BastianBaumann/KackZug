namespace KackZug.Core.Checking;

/// <summary>Eine Zeile der Abfahrtstafel von bahn.de</summary>
public sealed class BoardEntry
{
    public DateTime PlannedTime { get; set; }

    public DateTime? LiveTime { get; set; }

    public string? Platform { get; set; }

    public string? LivePlatform { get; set; }

    public string JourneyId { get; set; } = "";

    public string? Name { get; set; }

    public string? LineNumber { get; set; }

    public string? Category { get; set; }

    public string? Number { get; set; }

    public bool Cancelled { get; set; }
}
