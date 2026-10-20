namespace KackZug.ViewModels;

/// <summary>Wochenplan: welcher Tag welche Schicht hat</summary>
public partial class WeekViewModel : PageViewModel
{
    public WeekViewModel(MainViewModel main)
    {
        Main = main;
    }

    public MainViewModel Main { get; }

    public override string Title
    {
        get { return "Woche"; }
    }
}
