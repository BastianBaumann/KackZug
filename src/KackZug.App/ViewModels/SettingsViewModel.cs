using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core;
using KackZug.Core.Checking;
using KackZug.Core.Models;
using KackZug.Core.Planning;
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
        get { return "Einstellungen"; }
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
    public partial decimal? DelayThreshold { get; set; }

    [ObservableProperty]
    public partial decimal? MinTransfer { get; set; }

    [ObservableProperty]
    public partial bool InsistentAlerts { get; set; }

    [ObservableProperty]
    public partial bool CopyPreviousWeek { get; set; }

    [ObservableProperty]
    public partial bool StartWeekEmpty { get; set; }

    [ObservableProperty]
    public partial bool UpdateCheckEnabled { get; set; }

    [ObservableProperty]
    public partial bool ThemeSystem { get; set; }

    [ObservableProperty]
    public partial bool ThemeLight { get; set; }

    [ObservableProperty]
    public partial bool ThemeDark { get; set; }

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
        Main.ShowToast("Der Test-Alarm kommt in 5 Sekunden, auch wenn du die App schließt. Er geht nur mit „Gesehen“ weg.", false);
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
        DelayThreshold = settings.DelayThresholdMinutes;
        MinTransfer = settings.MinTransferMinutes;
        InsistentAlerts = settings.InsistentAlerts;
        CopyPreviousWeek = settings.NewWeekMode == NewWeekMode.CopyPrevious;
        StartWeekEmpty = !CopyPreviousWeek;
        UpdateCheckEnabled = settings.UpdateCheckEnabled;
        ThemeSystem = settings.ThemeMode == ThemeMode.System;
        ThemeLight = settings.ThemeMode == ThemeMode.Light;
        ThemeDark = settings.ThemeMode == ThemeMode.Dark;
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

    public void TidyEveningCheckTime()
    {
        EveningCheckTimeText = ClockText.Format(_eveningTime);
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

    partial void OnDelayThresholdChanged(decimal? value)
    {
        if (value is null)
        {
            return;
        }

        Settings.DelayThresholdMinutes = (int)value.Value;
        Save();
    }

    partial void OnMinTransferChanged(decimal? value)
    {
        if (value is null)
        {
            return;
        }

        Settings.MinTransferMinutes = (int)value.Value;
        Save();
    }

    partial void OnInsistentAlertsChanged(bool value)
    {
        Settings.InsistentAlerts = value;
        Save();
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

    partial void OnUpdateCheckEnabledChanged(bool value)
    {
        Settings.UpdateCheckEnabled = value;
        Save();
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
