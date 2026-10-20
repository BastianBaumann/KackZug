namespace KackZug.Core.Checking;

/// <summary>Gesamtergebnis einer Fahrt mit allen Teilstrecken und Umstiegen</summary>
public sealed class TripStatus
{
    public LegStatus Overall { get; set; } = LegStatus.OnTime;

    public bool HasLiveData { get; set; }

    public List<LegResult> Legs { get; } = new List<LegResult>();

    public List<TransferResult> Transfers { get; } = new List<TransferResult>();
}
