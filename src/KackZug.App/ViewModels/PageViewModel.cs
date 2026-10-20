namespace KackZug.ViewModels;

/// <summary>Basis fuer die Seiten der Navigation</summary>
public abstract class PageViewModel : ViewModelBase
{
    public abstract string Title { get; }

    public virtual void OnShown()
    {
    }
}
