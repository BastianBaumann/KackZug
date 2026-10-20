using CommunityToolkit.Mvvm.ComponentModel;

namespace KackZug.ViewModels;

/// <summary>Eine waehlbare Vorlaufzeit vor der Abfahrt</summary>
public partial class LeadTimeOption : ObservableObject
{
    private readonly Action _changed;

    public LeadTimeOption(int minutes, bool isChecked, Action changed)
    {
        Minutes = minutes;
        Text = minutes + " Min. vorher";
        if (minutes >= 60 && minutes % 60 == 0)
        {
            Text = (minutes / 60) + " Std. vorher";
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
