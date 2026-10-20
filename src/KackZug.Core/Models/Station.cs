namespace KackZug.Core.Models;

/// <summary>Ein Bahnhof oder eine Haltestelle mit ihrer Nummer bei der Bahn</summary>
public sealed class Station
{
    public string Name { get; set; } = "";

    public string EvaNumber { get; set; } = "";
}
