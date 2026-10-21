using KackZug.Core.Localization;

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
                    return Strings.Bahn_ErrorBlocked;
                case BahnApiErrorKind.NotFound:
                    return Strings.Bahn_ErrorNotFound;
                case BahnApiErrorKind.Network:
                    return Strings.Bahn_ErrorNetwork;
                default:
                    return Strings.Bahn_ErrorFormat;
            }
        }
    }
}
