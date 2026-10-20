using KackZug.Core.Models;

namespace KackZug.Core.Import;

/// <summary>Macht aus einem geteilten Text eine gespeicherte Verbindung</summary>
public interface IConnectionResolver
{
    Task<ResolvedConnection> ResolveAsync(string sharedText, CancellationToken cancellationToken);
}
