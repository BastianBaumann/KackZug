using KackZug.Core.Models;

namespace KackZug.Core.Sharing;

/// <summary>Liefert einen Teilen-Link fuer eine Verbindung an einem Tag, oder nichts, wenn es sie so nicht gibt</summary>
public interface IShareLinkCreator
{
    Task<string?> CreateAsync(SavedConnection connection, DateOnly travelDate, CancellationToken cancellationToken);
}
