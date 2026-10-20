using Avalonia.Media;
using KackZug.Core.Models;

namespace KackZug.ViewModels;

/// <summary>Eine Schicht in der Liste mit Kurzfassung der Fahrten</summary>
public sealed class ShiftCardViewModel
{
    public ShiftCardViewModel(Shift shift)
    {
        Shift = shift;
        Name = shift.Name;
        ColorBrush = ColorOption.BrushFor(shift.ColorHex);
        OutboundText = ConnectionText.Summary(shift.Outbound);
        ReturnText = ConnectionText.Summary(shift.Return);
    }

    public Shift Shift { get; }

    public string Name { get; }

    public IBrush ColorBrush { get; }

    public string OutboundText { get; }

    public string ReturnText { get; }
}
