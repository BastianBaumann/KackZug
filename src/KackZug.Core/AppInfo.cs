namespace KackZug.Core;

/// <summary>Feste Angaben zur App wie Name und Version</summary>
public static class AppInfo
{
    public const string Name = "KackZug";

    public static string Version
    {
        get
        {
            var version = typeof(AppInfo).Assembly.GetName().Version;
            if (version is null)
            {
                return "0.0.0";
            }

            return version.Major + "." + version.Minor + "." + version.Build;
        }
    }
}
