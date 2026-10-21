namespace KackZug.Core.Models;

/// <summary>Einstellungen des Nutzers</summary>
public sealed class Settings
{
    public List<int> LeadTimesMinutes { get; set; } = new List<int> { 90, 45, 15 };

    public TimeOnly? EveningCheckTime { get; set; } = new TimeOnly(20, 0);

    public bool EveningOkNotification { get; set; } = true;

    public int DelayThresholdMinutes { get; set; } = 5;

    public int MinTransferMinutes { get; set; } = 4;

    public bool InsistentAlerts { get; set; } = true;

    public bool BreakThroughDoNotDisturb { get; set; }

    public NewWeekMode NewWeekMode { get; set; } = NewWeekMode.CopyPrevious;

    public string AccentColor { get; set; } = "#C84B0C";

    public string SecondaryColor { get; set; } = "#8B5A2B";

    public ThemeMode ThemeMode { get; set; } = ThemeMode.System;

    public bool UpdateCheckEnabled { get; set; } = true;

    public string? TutorialSeenVersion { get; set; }
}
