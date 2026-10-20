using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            new NavItem("shifts", "Schichten", "IconTrain", () => new ShiftsViewModel(this)),
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

    public Task StartImportAsync(string sharedText)
    {
        var import = new ImportViewModel(this);
        Import = import;
        return import.LoadAsync(sharedText);
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
