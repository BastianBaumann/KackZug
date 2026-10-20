namespace KackZug.Core.Bahn;

/// <summary>Fehler bei einer Anfrage an bahn.de</summary>
public sealed class BahnApiException : Exception
{
    public BahnApiException(BahnApiErrorKind kind, string message) : base(message)
    {
        Kind = kind;
    }

    public BahnApiException(BahnApiErrorKind kind, string message, Exception innerException) : base(message, innerException)
    {
        Kind = kind;
    }

    public BahnApiErrorKind Kind { get; }

    public string UserMessage
    {
        get
        {
            switch (Kind)
            {
                case BahnApiErrorKind.Blocked:
                    return "bahn.de laesst gerade keine Anfragen zu. Bitte spaeter noch einmal versuchen.";
                case BahnApiErrorKind.NotFound:
                    return "Die Verbindung wurde bei bahn.de nicht gefunden. Bitte im DB Navigator neu teilen.";
                case BahnApiErrorKind.Network:
                    return "bahn.de ist gerade nicht erreichbar. Besteht eine Internetverbindung?";
                default:
                    return "Die Antwort von bahn.de konnte nicht gelesen werden.";
            }
        }
    }
}
