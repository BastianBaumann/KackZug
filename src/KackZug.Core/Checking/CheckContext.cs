namespace KackZug.Core.Checking;

/// <summary>Warum gerade geprueft wird, davon haengt ab, ob eine Info-Meldung kommt</summary>
public enum CheckContext
{
    Evening,
    LeadTime,
    LastLeadTime,
    Manual
}
