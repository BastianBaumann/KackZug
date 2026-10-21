using System.Collections.ObjectModel;
using AppUpdater;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core;
using KackZug.Localization;
using KackZug.Services;

namespace KackZug.ViewModels;

/// <summary>Rahmen der App: Navigation, Rueckfragen und kurze Hinweise</summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly Dictionary<string, PageViewModel> _pages = new Dictionary<string, PageViewModel>();
    private CancellationTokenSource? _toastTimer;

    public MainViewModel(AppServices services)
    {
        Services = services;
        NavItems = new ObservableCollection<NavItem>
        {
            new NavItem("week", UiStrings.Nav_Week, "IconWeek", () => new WeekViewModel(this)),
            new NavItem("trips", UiStrings.Nav_Trips, "IconTrain", () => new TripsViewModel(this)),
            new NavItem("shifts", UiStrings.Nav_Shifts, "IconShifts", () => new ShiftsViewModel(this)),
            new NavItem("settings", UiStrings.Nav_Settings, "IconSettings", () => new SettingsViewModel(this))
        };
        Navigate("week");
    }

    public AppServices Services { get; }

    public ObservableCollection<NavItem> NavItems { get; }

    [ObservableProperty]
    public partial PageViewModel? CurrentPage { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDialog))]
    public partial ConfirmViewModel? Dialog { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTutorial))]
    public partial TutorialViewModel? Tutorial { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImport))]
    public partial ImportViewModel? Import { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasToast))]
    public partial string? Toast { get; set; }

    [ObservableProperty]
    public partial bool ToastIsError { get; set; }

    public bool HasDialog
    {
        get { return Dialog is not null; }
    }

    public bool HasTutorial
    {
        get { return Tutorial is not null; }
    }

    public bool HasImport
    {
        get { return Import is not null; }
    }

    public bool HasToast
    {
        get { return !string.IsNullOrEmpty(Toast); }
    }

    [RelayCommand]
    public void Navigate(string key)
    {
        var item = NavItems.FirstOrDefault(navItem => navItem.Key == key);
        if (item is null)
        {
            return;
        }

        foreach (var navItem in NavItems)
        {
            navItem.IsSelected = navItem == item;
        }

        if (!_pages.TryGetValue(key, out var page))
        {
            page = item.Factory();
            _pages[key] = page;
        }

        CurrentPage = page;
        page.OnShown();
    }

    public void ShowTutorialIfNeeded()
    {
        var seenVersion = Services.Data.Settings.TutorialSeenVersion;
        if (seenVersion == AppInfo.Version)
        {
            return;
        }

        ShowTutorial(seenVersion is not null);
    }

    public void ShowTutorial(bool afterUpdate)
    {
        Tutorial = new TutorialViewModel(this, afterUpdate);
    }

    public void CloseTutorial()
    {
        Tutorial = null;
        Services.Data.Settings.TutorialSeenVersion = AppInfo.Version;
        Services.Store.Save(Services.Data);
    }

    public UpdateController Updates
    {
        get { return Services.Updates; }
    }

    public Task CheckForUpdateAsync()
    {
        return Updates.CheckIfDueAsync();
    }

    /// <summary>Laedt die neue Version und startet die Installation; ohne Installer auf dieser Plattform oeffnet sich die Download-Seite</summary>
    [RelayCommand]
    public async Task UpdateNowAsync()
    {
        var updates = Updates;
        var release = updates.Release;
        if (release is null || updates.IsDownloading)
        {
            return;
        }

        if (!updates.CanInstall)
        {
            Services.Platform.OpenUrl(release.PageUrl);
            return;
        }

        if (!updates.IsDownloaded)
        {
            try
            {
                await updates.DownloadAsync();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (UpdateVerificationException)
            {
                ShowToast(UiStrings.Update_Corrupt, true);
                return;
            }
            catch (Exception)
            {
                ShowToast(UiLanguage.Format(UiStrings.Update_DownloadFailed, UiStrings.Update_Now), true);
                return;
            }
        }

        await InstallUpdateAsync(true);
    }

    /// <summary>Nach der Rueckkehr in die App: Hat eine Installation auf die Erlaubnis gewartet, geht es jetzt weiter</summary>
    public async Task ContinueUpdateAfterPermissionAsync()
    {
        if (!Updates.TakeWaitingForPermission())
        {
            return;
        }

        await InstallUpdateAsync(false);
    }

    private async Task InstallUpdateAsync(bool askForPermission)
    {
        var result = await Updates.InstallAsync();
        if (result.Status == InstallStartStatus.Failed)
        {
            ShowToast(UiStrings.Update_InstallFailed, true);
            return;
        }

        if (result.Status != InstallStartStatus.NeedsPermission)
        {
            return;
        }

        if (!askForPermission)
        {
            ShowToast(UiStrings.Update_PermissionMissing, false);
            return;
        }

        var allowed = await ConfirmAsync(UiStrings.Update_PermissionTitle, UiStrings.Update_PermissionText, UiStrings.Settings_Allow);
        if (allowed)
        {
            Updates.OpenInstallPermissionSettings();
        }
    }

    /// <summary>Android meldet das Ende einer Installation, die die App nicht neu gestartet hat</summary>
    public void ReportInstallFinished(bool succeeded, bool aborted, string? message)
    {
        Updates.ReportInstallFinished(succeeded, message);
        if (succeeded)
        {
            return;
        }

        if (aborted)
        {
            ShowToast(UiStrings.Update_Aborted, false);
            return;
        }

        ShowToast(UiStrings.Update_NotInstalled, true);
    }

    [RelayCommand]
    private void DismissUpdate()
    {
        Updates.Dismiss();
    }

    public bool HandleBack()
    {
        if (Dialog is not null)
        {
            Dialog.CancelCommand.Execute(null);
            return true;
        }

        if (Import is not null)
        {
            CloseImport();
            return true;
        }

        if (CurrentPage is WeekViewModel week && week.HandleBack())
        {
            return true;
        }

        if (CurrentPage is TripsViewModel trips && trips.HandleBack())
        {
            return true;
        }

        if (CurrentPage is ShiftsViewModel shifts && shifts.HandleBack())
        {
            return true;
        }

        if (CurrentPage is not WeekViewModel)
        {
            Navigate("week");
            return true;
        }

        return false;
    }

    public Task StartImportAsync(string sharedText)
    {
        var import = new ImportViewModel(this);
        Import = import;
        return import.LoadAsync(sharedText);
    }

    public void StartManualEntry()
    {
        StartManualEntry(false);
    }

    /// <summary>Eingabe von Hand; aus dem Tab Fahrten ist danach gleich die Einzelfahrt gewaehlt</summary>
    public void StartManualEntry(bool asOneOffTrip)
    {
        var import = new ImportViewModel(this);
        import.PrefersOneOffTrip = asOneOffTrip;
        import.StartManual();
        Import = import;
    }

    public void CloseImport()
    {
        Import = null;
    }

    public async Task<bool> ConfirmAsync(string title, string message, string confirmText)
    {
        var dialog = new ConfirmViewModel(title, message, confirmText);
        Dialog = dialog;
        var result = await dialog.Result;
        Dialog = null;
        return result;
    }

    public void ShowToast(string message, bool isError)
    {
        _toastTimer?.Cancel();
        var timer = new CancellationTokenSource();
        _toastTimer = timer;
        Toast = message;
        ToastIsError = isError;

        var duration = TimeSpan.FromSeconds(3.5);
        if (isError)
        {
            duration = TimeSpan.FromSeconds(7);
        }

        _ = HideToastLaterAsync(duration, timer.Token);
    }

    private async Task HideToastLaterAsync(TimeSpan duration, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(duration, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(() => Toast = null);
    }
}
