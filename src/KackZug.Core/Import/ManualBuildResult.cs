using KackZug.Core.Models;

namespace KackZug.Core.Import;

/// <summary>Ergebnis der Eingabe von Hand: entweder eine Verbindung oder eine Meldung, was fehlt</summary>
public sealed class ManualBuildResult
{
    private ManualBuildResult(SavedConnection? connection, string? error)
    {
        Connection = connection;
        Error = error;
    }

    public SavedConnection? Connection { get; }

    public string? Error { get; }

    public static ManualBuildResult Success(SavedConnection connection)
    {
        return new ManualBuildResult(connection, null);
    }

    public static ManualBuildResult Failure(string error)
    {
        return new ManualBuildResult(null, error);
    }
}
