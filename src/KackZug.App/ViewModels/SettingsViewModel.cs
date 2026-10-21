using System.Collections.ObjectModel;
using Avalonia.Media;
using AppUpdater;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core;
using KackZug.Core.Checking;
using KackZug.Core.Localization;
using KackZug.Core.Models;
using KackZug.Core.Planning;
using KackZug.Localization;
using KackZug.Services;

namespace KackZug.ViewModels;

/// <summary>Einstellungen: Pruefzeiten, Alarm, Woche, Aussehen, Berechtigungen</summary>
public partial class SettingsViewModel : PageViewModel
{
    private static readonly int[] OfferedLeadTimes = { 120, 90, 60, 45, 30, 15 };

    private bool _isLoading;
    private TimeOnly _eveningTime = new TimeOnly(20, 0);

    public SettingsViewModel(MainViewModel main)
    {
        Main = main;
        foreach (var preset in ThemeService.Presets)
        {
            Presets.Add(new ColorPreset(preset.Name, preset.Accent));
        }

        Load();
        main.Services.DataChanged += (_, _) => RefreshPermissions();
    }

    public MainViewModel Main { get; }

    public override string Title
    {
        get { return UiStrings.Nav_Settings; }
    }

    public ObservableCollection<LeadTimeOption> LeadTimes { get; } = new ObservableCollection<LeadTimeOption>();

    public ObservableCollection<ColorPreset> Presets { get; } = new ObservableCollection<ColorPreset>();

    public ObservableCollection<PermissionInfo> Permissions { get; } = new ObservableCollection<PermissionInfo>();

    public string VersionText
    {
        get { return "KackZug " + AppInfo.Version + " · GPL-3.0 · github.com/BastianBaumann/KackZug"; }
    }

    [ObservableProperty]
    public partial bool EveningCheckEnabled { get; set; }

    [ObservableProperty]
    public partial string EveningCheckTimeText { get; set; } = "";

    [ObservableProperty]
    public partial bool EveningCheckTimeInvalid { get; set; }

    [ObservableProperty]
    public partial bool EveningOkNotification { get; set; }

    [ObservableProperty]
    public partial string DelayThresholdText { get; set; } = "";

    [ObservableProperty]
    public partial bool DelayThresholdInvalid { get; set; }

    [ObservableProperty]
    public partial string MinTransferText { get; set; } = "";

    [ObservableProperty]
    public partial bool MinTransferInvalid { get; set; }

    [ObservableProperty]
    public partial bool InsistentAlerts { get; set; }

    [ObservableProperty]
    public partial bool BreakThroughDoNotDisturb { get; set; }

    [ObservableProperty]
    public partial bool CopyPreviousWeek { get; set; }

    [ObservableProperty]
    public partial bool StartWeekEmpty { get; set; }

    [ObservableProperty]
    public partial bool UpdateCheckEnabled { get; set; }

    [ObservableProperty]
    public partial bool IncludePrereleases { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckUpdatesNowCommand))]
    public partial bool IsCheckingUpdates { get; set; }

    [ObservableProperty]
    public partial string? UpdateStatusText { get; set; }

    [ObservableProperty]
    public partial bool ThemeSystem { get; set; }

    [ObservableProperty]
    public partial bool ThemeLight { get; set; }

    [ObservableProperty]
    public partial bool ThemeDark { get; set; }

    [ObservableProperty]
    public partial bool LanguageSystem { get; set; }

    [ObservableProperty]
    public partial bool LanguageGerman { get; set; }

    [ObservableProperty]
    public partial bool LanguageEnglish { get; set; }

    [ObservableProperty]
    public partial Color AccentColor { get; set; }

    [ObservableProperty]
    public partial bool HasNoPermissions { get; set; }

    [ObservableProperty]
    public partial bool HasMissingPermissions { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TestAlarmCommand))]
    public partial bool TestAlarmPending { get; set; }

    public override void OnShown()
    {
        RefreshPermissions();
    }

    [RelayCommand]
    private void ApplyPreset(ColorPreset preset)
    {
        _isLoading = true;
        AccentColor = Color.Parse(preset.Accent);
        _isLoading = false;
        SaveColors();
    }

    [RelayCommand]
    private void FixPermission(PermissionInfo permission)
    {
        Main.Services.Platform.OpenPermissionSettings(permission.Key);
    }

    [RelayCommand]
    private void ShowTutorial()
    {
        Main.ShowTutorial(false);
    }

    [RelayCommand(CanExecute = nameof(CanTestAlarm))]
    private async Task TestAlarmAsync()
    {
        TestAlarmPending = true;
        Main.Services.Platform.ScheduleTestAlarm(TimeSpan.FromSeconds(5));
        Main.ShowToast(UiStrings.Settings_TestAlarmToast, false);
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        finally
        {
            TestAlarmPending = false;
        }
    }

    private bool CanTestAlarm()
    {
        return !TestAlarmPending;
    }

    private Settings Settings
    {
        get { return Main.Services.Data.Settings; }
    }

    private void Load()
    {
        _isLoading = true;
        var settings = Settings;

        LeadTimes.Clear();
        var allMinutes = OfferedLeadTimes.Union(settings.LeadTimesMinutes).OrderByDescending(minutes => minutes);
        foreach (var minutes in allMinutes)
        {
            LeadTimes.Add(new LeadTimeOption(minutes, settings.LeadTimesMinutes.Contains(minutes), SaveLeadTimes));
        }

        EveningCheckEnabled = settings.EveningCheckTime is not null;
        if (settings.EveningCheckTime is not null)
        {
            _eveningTime = settings.EveningCheckTime.Value;
        }

        EveningCheckTimeText = ClockText.Format(_eveningTime);
        EveningCheckTimeInvalid = false;
        EveningOkNotification = settings.EveningOkNotification;
        DelayThresholdText = settings.DelayThresholdMinutes.ToString();
        DelayThresholdInvalid = false;
        MinTransferText = settings.MinTransferMinutes.ToString();
        MinTransferInvalid = false;
        InsistentAlerts = settings.InsistentAlerts;
        BreakThroughDoNotDisturb = settings.BreakThroughDoNotDisturb;
        CopyPreviousWeek = settings.NewWeekMode == NewWeekMode.CopyPrevious;
        StartWeekEmpty = !CopyPreviousWeek;
        UpdateCheckEnabled = settings.UpdateCheckEnabled;
        IncludePrereleases = settings.IncludePrereleases;
        ThemeSystem = settings.ThemeMode == ThemeMode.System;
        ThemeLight = settings.ThemeMode == ThemeMode.Light;
        ThemeDark = settings.ThemeMode == ThemeMode.Dark;
        LanguageSystem = settings.Language == AppLanguage.System;
        LanguageGerman = settings.Language == AppLanguage.German;
        LanguageEnglish = settings.Language == AppLanguage.English;
        AccentColor = ThemeService.Parse(settings.AccentColor, ThemeService.DefaultAccent);
        _isLoading = false;
        RefreshPermissions();
    }

    private void RefreshPermissions()
    {
        Permissions.Clear();
        foreach (var permission in Main.Services.Platform.GetPermissions())
        {
            Permissions.Add(permission);
        }

        HasMissingPermissions = Permissions.Any(permission => permission.Missing);
        HasNoPermissions = Permissions.Count == 0;
    }

    private void SaveLeadTimes()
    {
        if (_isLoading)
        {
            return;
        }

        Settings.LeadTimesMinutes = LeadTimes.Where(option => option.IsChecked).Select(option => option.Minutes).OrderByDescending(minutes => minutes).ToList();
        Save();
    }

    private void Save()
    {
        if (_isLoading)
        {
            return;
        }

        Main.Services.SaveAndReschedule();
    }

    private void SaveColors()
    {
        if (_isLoading)
        {
            return;
        }

        Settings.AccentColor = ThemeService.ToHex(AccentColor);
        Main.Services.Theme.Apply(Settings);
        Main.Services.Store.Save(Main.Services.Data);
    }

    private void SaveTheme(ThemeMode mode)
    {
        if (_isLoading)
        {
            return;
        }

        Settings.ThemeMode = mode;
        Main.Services.Theme.Apply(Settings);
        Main.Services.Store.Save(Main.Services.Data);
    }

    partial void OnEveningCheckEnabledChanged(bool value)
    {
        SaveEveningCheck();
    }

    partial void OnEveningCheckTimeTextChanged(string value)
    {
        var time = ClockText.Parse(value);
        EveningCheckTimeInvalid = time is null;
        if (time is null || time.Value == _eveningTime)
        {
            return;
        }

        _eveningTime = time.Value;
        SaveEveningCheck();
    }

    public void TidyTextFields()
    {
        EveningCheckTimeText = ClockText.Format(_eveningTime);
        DelayThresholdText = Settings.DelayThresholdMinutes.ToString();
        MinTransferText = Settings.MinTransferMinutes.ToString();
    }

    private void SaveEveningCheck()
    {
        if (_isLoading)
        {
            return;
        }

        Settings.EveningCheckTime = null;
        if (EveningCheckEnabled)
        {
            Settings.EveningCheckTime = _eveningTime;
        }

        Save();
    }

    partial void OnEveningOkNotificationChanged(bool value)
    {
        Settings.EveningOkNotification = value;
        Save();
    }

    partial void OnDelayThresholdTextChanged(string value)
    {
        var minutes = ParseMinutes(value, 1, 60);
        DelayThresholdInvalid = minutes is null;
        if (minutes is null || minutes.Value == Settings.DelayThresholdMinutes)
        {
            return;
        }

        Settings.DelayThresholdMinutes = minutes.Value;
        Save();
    }

    partial void OnMinTransferTextChanged(string value)
    {
        var minutes = ParseMinutes(value, 0, 30);
        MinTransferInvalid = minutes is null;
        if (minutes is null || minutes.Value == Settings.MinTransferMinutes)
        {
            return;
        }

        Settings.MinTransferMinutes = minutes.Value;
        Save();
    }

    private static int? ParseMinutes(string text, int minimum, int maximum)
    {
        var trimmed = text.Trim();
        if (trimmed.Length == 0 || trimmed.Length > 3 || !trimmed.All(char.IsAsciiDigit))
        {
            return null;
        }

        var minutes = int.Parse(trimmed);
        if (minutes < minimum || minutes > maximum)
        {
            return null;
        }

        return minutes;
    }

    partial void OnInsistentAlertsChanged(bool value)
    {
        Settings.InsistentAlerts = value;
        Save();
    }

    partial void OnBreakThroughDoNotDisturbChanged(bool value)
    {
        Settings.BreakThroughDoNotDisturb = value;
        if (_isLoading)
        {
            return;
        }

        Main.Services.Store.Save(Main.Services.Data);
        RefreshPermissions();
        var missing = Permissions.FirstOrDefault(permission => permission.Key == "dnd" && permission.Missing);
        if (value && missing is not null)
        {
            Main.Services.Platform.OpenPermissionSettings(missing.Key);
        }
    }

    partial void OnCopyPreviousWeekChanged(bool value)
    {
        if (value)
        {
            Settings.NewWeekMode = NewWeekMode.CopyPrevious;
            Save();
        }
    }

    partial void OnStartWeekEmptyChanged(bool value)
    {
        if (value)
        {
            Settings.NewWeekMode = NewWeekMode.Empty;
            Save();
        }
    }

    [RelayCommand(CanExecute = nameof(CanCheckUpdatesNow))]
    public async Task CheckUpdatesNowAsync()
    {
        IsCheckingUpdates = true;
        UpdateStatusText = UiStrings.Settings_UpdateChecking;
        try
        {
            var updates = Main.Services.Updates;
            var result = await updates.CheckAsync();
            UpdateStatusText = DescribeCheck(result, updates.ActionText);
        }
        finally
        {
            IsCheckingUpdates = false;
        }
    }

    private static string? DescribeCheck(UpdateCheckResult? result, string actionText)
    {
        if (result is null)
        {
            return null;
        }

        if (result.Outcome == UpdateCheckOutcome.UpdateAvailable)
        {
            return "Version " + result.Release!.Version + " ist da. Oben auf „" + actionText + "“ tippen.";
        }

        if (result.Outcome == UpdateCheckOutcome.UpToDate)
        {
            return "Du hast die neueste Version (" + AppInfo.FullVersion + ").";
        }

        if (result.FailureReason == UpdateFailureReason.RateLimited)
        {
            return "GitHub lässt gerade keine Abfragen zu. Versuch es in einer Stunde noch mal.";
        }

        if (result.FailureReason == UpdateFailureReason.NoMatchingAsset)
        {
            return "Zur neuesten Version gibt es noch keine App-Datei. Versuch es später noch mal.";
        }

        if (result.FailureReason == UpdateFailureReason.InvalidResponse)
        {
            return "Konnte bei GitHub nicht nachsehen. Ist das Projekt öffentlich?";
        }

        return "Konnte bei GitHub nicht nachsehen. Internet da?";
    }

    private bool CanCheckUpdatesNow()
    {
        return !IsCheckingUpdates;
    }

    partial void OnUpdateCheckEnabledChanged(bool value)
    {
        Settings.UpdateCheckEnabled = value;
        Save();
    }

    partial void OnIncludePrereleasesChanged(bool value)
    {
        Settings.IncludePrereleases = value;
        Save();
    }

    partial void OnLanguageSystemChanged(bool value)
    {
        if (value)
        {
            SaveLanguage(AppLanguage.System);
        }
    }

    partial void OnLanguageGermanChanged(bool value)
    {
        if (value)
        {
            SaveLanguage(AppLanguage.German);
        }
    }

    partial void OnLanguageEnglishChanged(bool value)
    {
        if (value)
        {
            SaveLanguage(AppLanguage.English);
        }
    }

    /// <remarks>Die Oberflaeche liest ihre Texte beim Aufbau, deshalb wird sie nach dem Wechsel neu gebaut.</remarks>
    private void SaveLanguage(AppLanguage language)
    {
        if (_isLoading || Settings.Language == language)
        {
            return;
        }

        var before = Language.Culture;
        Settings.Language = language;
        Main.Services.SaveAndReschedule();
        UiLanguage.Apply(language);
        if (Language.Culture.Name != before.Name)
        {
            App.ReloadUi();
        }
    }

    partial void OnThemeSystemChanged(bool value)
    {
        if (value)
        {
            SaveTheme(ThemeMode.System);
        }
    }

    partial void OnThemeLightChanged(bool value)
    {
        if (value)
        {
            SaveTheme(ThemeMode.Light);
        }
    }

    partial void OnThemeDarkChanged(bool value)
    {
        if (value)
        {
            SaveTheme(ThemeMode.Dark);
        }
    }

    partial void OnAccentColorChanged(Color value)
    {
        SaveColors();
    }

}
