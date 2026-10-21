namespace KackZug.Core.Updates;

/// <summary>Was die Suche nach einer neuen Version ergeben hat</summary>
public enum UpdateCheckOutcome
{
    NewVersion,
    UpToDate,
    Failed
}
