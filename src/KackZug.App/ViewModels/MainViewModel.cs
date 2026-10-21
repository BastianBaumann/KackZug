using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core;
using KackZug.Core.Updates;
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
            new NavItem("week", "Woche", "IconWeek", () => new WeekViewModel(this)),
            new NavItem("shifts", "Schichten", "IconShifts", () => new ShiftsViewModel(this)),
            new NavItem("settings", "Einstellungen", "IconSettings", () => new SettingsViewModel(this))
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
    [NotifyPropertyChangedFor(nameof(HasUpdate))]
    public partial UpdateInfo? AvailableUpdate { get; set; }

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

    public bool HasUpdate
    {
        get { return AvailableUpdate is not null; }
    }

    public string UpdateText
    {
        get
        {
            if (AvailableUpdate is null)
            {
                return "";
            }

            return "Version " + AvailableUpdate.Version + " ist da.";
        }
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

    public async Task CheckForUpdateAsync()
    {
        var data = Services.Data;
        if (!data.Settings.UpdateCheckEnabled || !UpdateChecker.IsDue(data.LastUpdateCheck, Services.Clock.Now))
        {
            return;
        }

        data.LastUpdateCheck = Services.Clock.Now;
        Services.Store.Save(data);
        var checker = Services.CreateUpdateChecker();
        var update = await checker.FindNewerAsync(AppInfo.Version, CancellationToken.None);
        if (update is not null)
        {
            ShowUpdate(update);
        }
    }

    public void ShowUpdate(UpdateInfo update)
    {
        AvailableUpdate = update;
        OnPropertyChanged(nameof(UpdateText));
    }

    [RelayCommand]
    private void OpenUpdatePage()
    {
        if (AvailableUpdate is not null)
        {
            Services.Platform.OpenUrl(AvailableUpdate.PageUrl);
        }
    }

    [RelayCommand]
    private void DismissUpdate()
    {
        AvailableUpdate = null;
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
        var import = new ImportViewModel(this);
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
