using KackZug.Core.Models;

namespace KackZug.Core.Import;

/// <summary>Sucht Bahnhoefe und Haltestellen nach Namen, fuer die Eingabe von Hand</summary>
public interface IStationSearch
{
    Task<IReadOnlyList<Station>> SearchAsync(string term, CancellationToken cancellationToken);
}
