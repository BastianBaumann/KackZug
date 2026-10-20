namespace KackZug.ViewModels;

/// <summary>Einstellungen der App</summary>
public partial class SettingsViewModel : PageViewModel
{
    public SettingsViewModel(MainViewModel main)
    {
        Main = main;
    }

    public MainViewModel Main { get; }

    public override string Title
    {
        get { return "Einstellungen"; }
    }
}
