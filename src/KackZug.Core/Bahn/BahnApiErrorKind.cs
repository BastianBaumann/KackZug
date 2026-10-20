namespace KackZug.Core.Bahn;

/// <summary>Art eines Fehlers bei bahn.de</summary>
public enum BahnApiErrorKind
{
    Network,
    Blocked,
    NotFound,
    Format
}
