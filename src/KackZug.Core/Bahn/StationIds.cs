using KackZug.Core.Models;

namespace KackZug.Core.Bahn;

/// <summary>Schreibweise, mit der bahn.de einen Halt in Anfragen erwartet</summary>
public static class StationIds
{
    public static string For(Station station)
    {
        return "A=1@O=" + station.Name + "@L=" + station.EvaNumber + "@";
    }
}
