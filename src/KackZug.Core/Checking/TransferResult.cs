namespace KackZug.Core.Checking;

/// <summary>Bewertung eines Umstiegs zwischen zwei Fahrten</summary>
public sealed class TransferResult
{
    public int ArrivingLegIndex { get; set; }

    public int DepartingLegIndex { get; set; }

    public int SpareMinutes { get; set; }

    public bool AtRisk { get; set; }
}
