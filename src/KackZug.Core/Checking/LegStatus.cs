namespace KackZug.Core.Checking;

/// <summary>Ergebnis fuer eine Teilstrecke oder Fahrt, sortiert von harmlos nach schlimm</summary>
public enum LegStatus
{
    OnTime = 0,
    Unknown = 1,
    PlatformChanged = 2,
    Delayed = 3,
    TransferAtRisk = 4,
    NotFound = 5,
    PartlyCancelled = 6,
    Cancelled = 7
}
