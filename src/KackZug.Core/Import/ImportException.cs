namespace KackZug.Core.Import;

/// <summary>Fehler beim Uebernehmen einer geteilten Verbindung, mit verstaendlicher Meldung</summary>
public sealed class ImportException : Exception
{
    public ImportException(string message) : base(message)
    {
    }

    public ImportException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
