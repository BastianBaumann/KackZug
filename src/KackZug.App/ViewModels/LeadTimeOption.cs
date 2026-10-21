using CommunityToolkit.Mvvm.ComponentModel;
using KackZug.Localization;

namespace KackZug.ViewModels;

/// <summary>Eine waehlbare Vorlaufzeit vor der Abfahrt</summary>
public partial class LeadTimeOption : ObservableObject
{
    private readonly Action _changed;

    public LeadTimeOption(int minutes, bool isChecked, Action changed)
    {
        Minutes = minutes;
        Text = UiLanguage.Format(UiStrings.Settings_LeadMinutes, minutes);
        if (minutes >= 60 && minutes % 60 == 0)
        {
            Text = UiLanguage.Format(UiStrings.Settings_LeadHours, minutes / 60);
        }

        IsChecked = isChecked;
        _changed = changed;
    }

    public int Minutes { get; }

    public string Text { get; }

    [ObservableProperty]
    public partial bool IsChecked { get; set; }

    partial void OnIsCheckedChanged(bool value)
    {
        _changed?.Invoke();
    }
}
