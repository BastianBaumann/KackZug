using Avalonia;
using Avalonia.Media;
using KackZug.Core.Checking;

namespace KackZug.ViewModels;

/// <summary>Farbiger Punkt mit Kurztext fuer den letzten bekannten Stand einer Fahrt</summary>
public sealed class StatusBadge
{
    public StatusBadge(LegStatus? status, string text)
    {
        Text = text;
        Brush = BrushFor(status);
    }

    public string Text { get; }

    public IBrush Brush { get; }

    public static string ShortText(LegStatus? status)
    {
        if (status is null)
        {
            return "noch nicht geprüft";
        }

        switch (status.Value)
        {
            case LegStatus.OnTime:
                return "planmäßig";
            case LegStatus.Unknown:
                return "konnte nicht prüfen";
            case LegStatus.PlatformChanged:
                return "Gleiswechsel";
            case LegStatus.Delayed:
                return "verspätet";
            case LegStatus.TransferAtRisk:
                return "Anschluss gefährdet";
            case LegStatus.NotFound:
                return "fährt so nicht";
            case LegStatus.PartlyCancelled:
                return "Teilausfall";
            default:
                return "fällt aus";
        }
    }

    private static IBrush BrushFor(LegStatus? status)
    {
        var key = "AppNeutralBrush";
        if (status is not null)
        {
            switch (status.Value)
            {
                case LegStatus.OnTime:
                    key = "AppSuccessBrush";
                    break;
                case LegStatus.PlatformChanged:
                case LegStatus.Delayed:
                    key = "AppWarningBrush";
                    break;
                case LegStatus.TransferAtRisk:
                case LegStatus.NotFound:
                case LegStatus.PartlyCancelled:
                case LegStatus.Cancelled:
                    key = "AppDangerBrush";
                    break;
            }
        }

        var application = Application.Current;
        if (application is not null && application.TryGetResource(key, null, out var resource) && resource is IBrush brush)
        {
            return brush;
        }

        return Brushes.Gray;
    }
}
