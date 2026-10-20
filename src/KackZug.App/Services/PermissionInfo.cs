namespace KackZug.Services;

/// <summary>Stand einer Berechtigung, die KackZug fuer zuverlaessige Alarme braucht</summary>
public sealed class PermissionInfo
{
    public PermissionInfo(string key, string title, string explanation, bool granted)
    {
        Key = key;
        Title = title;
        Explanation = explanation;
        Granted = granted;
    }

    public string Key { get; }

    public string Title { get; }

    public string Explanation { get; }

    public bool Granted { get; }

    public bool Missing
    {
        get { return !Granted; }
    }
}
