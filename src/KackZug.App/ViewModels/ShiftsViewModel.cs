namespace KackZug.ViewModels;

/// <summary>Liste der Schichten mit Hin- und Rueckfahrt</summary>
public partial class ShiftsViewModel : PageViewModel
{
    public ShiftsViewModel(MainViewModel main)
    {
        Main = main;
    }

    public MainViewModel Main { get; }

    public override string Title
    {
        get { return "Schichten"; }
    }
}
