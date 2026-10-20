using KackZug.Core.Models;

namespace KackZug.Core.Import;

/// <summary>Ergebnis eines Imports: die Verbindung und der Tag, an dem sie geteilt wurde</summary>
public sealed class ResolvedConnection
{
    public ResolvedConnection(SavedConnection connection, DateOnly travelDate, bool hasPlatforms)
    {
        Connection = connection;
        TravelDate = travelDate;
        HasPlatforms = hasPlatforms;
    }

    public SavedConnection Connection { get; }

    public DateOnly TravelDate { get; }

    public bool HasPlatforms { get; }
}
