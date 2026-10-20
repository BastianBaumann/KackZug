namespace KackZug.Core.Updates;

/// <summary>Eine neuere Version auf GitHub und die Seite, auf der sie zu finden ist</summary>
public sealed class UpdateInfo
{
    public UpdateInfo(string version, string pageUrl)
    {
        Version = version;
        PageUrl = pageUrl;
    }

    public string Version { get; }

    public string PageUrl { get; }
}
