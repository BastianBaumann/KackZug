using System.Reflection;

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

    /// <summary>Version mit Vorabkennung wie "0.3.0-dev.2", damit Vorabversionen beim Update richtig verglichen werden</summary>
    public static string FullVersion
    {
        get
        {
            var attribute = typeof(AppInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return CleanFullVersion(attribute?.InformationalVersion, Version);
        }
    }

    /// <summary>Entfernt Build-Angaben hinter "+"; ohne brauchbare Angabe bleibt die einfache Version</summary>
    public static string CleanFullVersion(string? informationalVersion, string fallback)
    {
        if (string.IsNullOrWhiteSpace(informationalVersion))
        {
            return fallback;
        }

        var cleaned = informationalVersion.Trim();
        var plus = cleaned.IndexOf('+');
        if (plus >= 0)
        {
            cleaned = cleaned.Substring(0, plus);
        }

        if (cleaned.Length == 0 || !char.IsDigit(cleaned[0]))
        {
            return fallback;
        }

        return cleaned;
    }
}
