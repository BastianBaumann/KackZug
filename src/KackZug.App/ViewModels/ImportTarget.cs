using CommunityToolkit.Mvvm.ComponentModel;
using KackZug.Core.Models;

namespace KackZug.ViewModels;

/// <summary>Moegliches Ziel eines Imports: eine bestehende Schicht oder eine neue</summary>
public partial class ImportTarget : ObservableObject
{
    public ImportTarget(Shift? shift)
    {
        Shift = shift;
        if (shift is null)
        {
            Title = "Neue Schicht";
        }
        else
        {
            Title = shift.Name;
        }
    }

    public Shift? Shift { get; }

    public string Title { get; }

    public bool IsNew
    {
        get { return Shift is null; }
    }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
