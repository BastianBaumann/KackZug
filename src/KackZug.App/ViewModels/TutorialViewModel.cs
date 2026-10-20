using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core;

namespace KackZug.ViewModels;

/// <summary>Kurze Einfuehrung beim ersten Start und nach jedem Update</summary>
public partial class TutorialViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public TutorialViewModel(MainViewModel main, bool afterUpdate)
    {
        _main = main;
        var welcome = "KackZug prüft deine Zugverbindungen zur Arbeit und meldet sich laut, wenn etwas nicht stimmt.";
        if (afterUpdate)
        {
            welcome = "KackZug wurde auf Version " + AppInfo.Version + " aktualisiert. Hier noch einmal kurz das Wichtigste.";
        }

        Steps.Add(new TutorialStep("Willkommen bei KackZug", welcome, null));
        Steps.Add(new TutorialStep("Verbindung teilen",
            "Such im DB Navigator deine Verbindung zur Arbeit, tippe auf „Teilen“ und wähle KackZug. Dann speicherst du sie als Hin- oder Rückfahrt einer Schicht, zum Beispiel „Frühdienst“.",
            "IconShare"));
        Steps.Add(new TutorialStep("Woche planen",
            "Unter „Woche“ gibst du jedem Tag eine Schicht oder lässt ihn frei. Hin- und Rückfahrt kannst du pro Tag einzeln abschalten. Eine neue Woche übernimmt auf Wunsch den Plan der Vorwoche.",
            "IconWeek"));
        Steps.Add(new TutorialStep("Prüfen und melden",
            "KackZug prüft am Abend vorher und kurz vor der Abfahrt. Fällt ein Zug aus, ist er verspätet, wechselt das Gleis oder wird der Anschluss knapp, kommt ein lauter Alarm.",
            "IconBell"));
        Steps.Add(new TutorialStep("Damit Alarme ankommen",
            "Erlaube Benachrichtigungen und nimm KackZug aus der Akku-Optimierung. Unter „Einstellungen“ siehst du, was noch fehlt, und kannst einen Test-Alarm auslösen.",
            "IconSettings"));
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
