namespace KackZug.Core.Checking;

/// <summary>Ergebnis fuer eine Teilstrecke oder Fahrt, sortiert von harmlos nach schlimm; PlannedOnly heisst laut Fahrplan ok, aber noch ohne Echtzeit</summary>
public enum LegStatus
{
    OnTime = 0,
    PlannedOnly = 1,
    Unknown = 2,
    PlatformChanged = 3,
    Delayed = 4,
    TransferAtRisk = 5,
    NotFound = 6,
    PartlyCancelled = 7,
    Cancelled = 8
}
