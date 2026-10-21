using KackZug.Core.Models;

namespace KackZug.Core.Import;

/// <summary>Eine von Hand eingegebene Teilstrecke, so wie sie im Formular steht</summary>
public sealed class ManualLegInput
{
    public Station? From { get; set; }

    public Station? To { get; set; }

    public string DepartureText { get; set; } = "";

    public string ArrivalText { get; set; } = "";

    public string LineText { get; set; } = "";

    public string TrainNumberText { get; set; } = "";
}
