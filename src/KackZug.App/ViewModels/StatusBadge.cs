using Avalonia;
using Avalonia.Media;
using KackZug.Core.Checking;
using KackZug.Core.Localization;
using KackZug.Core.Models;
using KackZug.Localization;
using KackZug.Services;

namespace KackZug.ViewModels;

/// <summary>Status-Kennzeichen wie im Designsystem: Flaeche, Symbol und Wort, nie nur Farbe</summary>
public sealed class StatusBadge
{
    public StatusBadge(LegStatus? status, string text, string caption)
    {
        Text = text;
        Caption = caption;
        var variant = VariantOf(status);
        Background = ThemeService.FindBrush("App" + variant + "Brush", Brushes.Gray);
        Foreground = ThemeService.FindBrush("AppOn" + variant + "Brush", Brushes.White);
        Icon = FindGeometry(IconKeyOf(status));
    }

    public string Text { get; }

    public string Caption { get; }

    public IBrush Background { get; }

    public IBrush Foreground { get; }

    public Geometry? Icon { get; }

    /// <remarks>Ein gespeicherter Text aus einer anderen Sprache wird durch den kurzen Text in der aktuellen Sprache ersetzt.</remarks>
    public static string TextFor(CheckState state)
    {
        var language = state.LastBadgeLanguage;
        if (string.IsNullOrEmpty(language))
        {
            language = "de";
        }

        if (string.IsNullOrEmpty(state.LastBadgeText) || language != Language.Culture.TwoLetterISOLanguageName)
        {
            return ShortText(state.LastStatus);
        }

        return state.LastBadgeText;
    }

    public static string ShortText(LegStatus? status)
    {
        if (status is null)
        {
            return UiStrings.Status_NotChecked;
        }

        switch (status.Value)
        {
            case LegStatus.OnTime:
                return UiStrings.Status_OnTime;
            case LegStatus.PlannedOnly:
                return UiStrings.Status_PlannedOnly;
            case LegStatus.Unknown:
                return UiStrings.Status_Unknown;
            case LegStatus.PlatformChanged:
                return UiStrings.Status_PlatformChanged;
            case LegStatus.Delayed:
                return UiStrings.Status_Delayed;
            case LegStatus.TransferAtRisk:
                return UiStrings.Status_TransferAtRisk;
            case LegStatus.NotFound:
                return UiStrings.Status_NotFound;
            case LegStatus.PartlyCancelled:
                return UiStrings.Status_PartlyCancelled;
            default:
                return UiStrings.Status_Cancelled;
        }
    }

    private static string VariantOf(LegStatus? status)
    {
        if (status is null)
        {
            return "Unknown";
        }

        switch (status.Value)
        {
            case LegStatus.OnTime:
            case LegStatus.PlannedOnly:
                return "Ok";
            case LegStatus.PlatformChanged:
            case LegStatus.Delayed:
            case LegStatus.TransferAtRisk:
                return "Late";
            case LegStatus.NotFound:
            case LegStatus.PartlyCancelled:
            case LegStatus.Cancelled:
                return "Cancel";
            default:
                return "Unknown";
        }
    }

    private static string IconKeyOf(LegStatus? status)
    {
        if (status is null)
        {
            return "IconUnknown";
        }

        switch (status.Value)
        {
            case LegStatus.OnTime:
            case LegStatus.PlannedOnly:
                return "IconOk";
            case LegStatus.Delayed:
                return "IconLate";
            case LegStatus.PlatformChanged:
                return "IconPlatform";
            case LegStatus.TransferAtRisk:
                return "IconTransfer";
            case LegStatus.NotFound:
                return "IconWarning";
            case LegStatus.PartlyCancelled:
            case LegStatus.Cancelled:
                return "IconCancelled";
            default:
                return "IconUnknown";
        }
    }

    private static Geometry? FindGeometry(string key)
    {
        var application = Application.Current;
        if (application is not null && application.TryGetResource(key, null, out var resource))
        {
            return resource as Geometry;
        }

        return null;
    }
}
