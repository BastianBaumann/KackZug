using KackZug.Core.Localization;
using KackZug.Core.Models;

namespace KackZug.Localization;

/// <summary>Stellt die Sprache fuer Kern und Oberflaeche zusammen um</summary>
public static class UiLanguage
{
    public static void Apply(AppLanguage language)
    {
        Language.Apply(language);
        UiStrings.Culture = Language.Culture;
    }

    public static string Format(string format, params object?[] arguments)
    {
        return string.Format(Language.Culture, format, arguments);
    }
}
