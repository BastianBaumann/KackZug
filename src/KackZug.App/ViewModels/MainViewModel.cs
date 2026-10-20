using KackZug.Core;

namespace KackZug.ViewModels;

/// <summary>Hauptansicht der App</summary>
public partial class MainViewModel : ViewModelBase
{
    public string Title
    {
        get { return AppInfo.Name; }
    }

    public string VersionText
    {
        get { return "Version " + AppInfo.Version; }
    }
}
