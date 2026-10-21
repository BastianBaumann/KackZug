using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core;
using KackZug.Localization;

namespace KackZug.ViewModels;

/// <summary>Kurze Einfuehrung beim ersten Start und nach jedem Update</summary>
public partial class TutorialViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public TutorialViewModel(MainViewModel main, bool afterUpdate)
    {
        _main = main;
        var welcome = UiStrings.Tutorial_WelcomeText;
        if (afterUpdate)
        {
            welcome = UiLanguage.Format(UiStrings.Tutorial_UpdatedText, AppInfo.Version);
        }

        Steps.Add(new TutorialStep(UiStrings.Tutorial_WelcomeTitle, welcome, null));
        Steps.Add(new TutorialStep(UiStrings.Tutorial_ShareTitle, UiStrings.Tutorial_ShareText, "IconShare"));
        Steps.Add(new TutorialStep(UiStrings.Tutorial_WeekTitle, UiStrings.Tutorial_WeekText, "IconWeek"));
        Steps.Add(new TutorialStep(UiStrings.Tutorial_CheckTitle, UiStrings.Tutorial_CheckText, "IconBell"));
        Steps.Add(new TutorialStep(UiStrings.Tutorial_AlarmsTitle, UiStrings.Tutorial_AlarmsText, "IconSettings"));
        Steps[0].IsCurrent = true;
    }

    public ObservableCollection<TutorialStep> Steps { get; } = new ObservableCollection<TutorialStep>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Current))]
    [NotifyPropertyChangedFor(nameof(IsFirst))]
    [NotifyPropertyChangedFor(nameof(IsLast))]
    [NotifyPropertyChangedFor(nameof(IsNotLast))]
    public partial int Index { get; set; }

    public TutorialStep Current
    {
        get { return Steps[Index]; }
    }

    public bool IsFirst
    {
        get { return Index == 0; }
    }

    public bool IsLast
    {
        get { return Index == Steps.Count - 1; }
    }

    public bool IsNotLast
    {
        get { return !IsLast; }
    }

    [RelayCommand]
    private void Next()
    {
        if (!IsLast)
        {
            Index++;
        }
    }

    [RelayCommand]
    private void Back()
    {
        if (!IsFirst)
        {
            Index--;
        }
    }

    [RelayCommand]
    private void Finish()
    {
        _main.CloseTutorial();
    }

    [RelayCommand]
    private void OpenSettings()
    {
        _main.CloseTutorial();
        _main.Navigate("settings");
    }

    partial void OnIndexChanged(int value)
    {
        for (var position = 0; position < Steps.Count; position++)
        {
            Steps[position].IsCurrent = position == value;
        }
    }
}
