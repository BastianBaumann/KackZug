using System.Globalization;
using KackZug.Core.Models;

namespace KackZug.Core.Localization;

/// <summary>Stellt die Sprache fuer alle Texte, Datums- und Wochentagsnamen ein</summary>
public static class Language
{
    private static readonly CultureInfo GermanCulture = new CultureInfo("de-DE");
    private static readonly CultureInfo EnglishCulture = new CultureInfo("en-GB");

    public static CultureInfo Culture { get; private set; } = GermanCulture;

    public static bool IsGerman
    {
        get { return Culture.TwoLetterISOLanguageName == "de"; }
    }

    public static void Apply(AppLanguage language)
    {
        Apply(language, CultureInfo.InstalledUICulture);
    }

    public static void Apply(AppLanguage language, CultureInfo systemCulture)
    {
        Culture = Resolve(language, systemCulture);
        Strings.Culture = Culture;
        CultureInfo.DefaultThreadCurrentUICulture = Culture;
        CultureInfo.CurrentUICulture = Culture;
    }

    public static CultureInfo Resolve(AppLanguage language, CultureInfo systemCulture)
    {
        switch (language)
        {
            case AppLanguage.German:
                return GermanCulture;
            case AppLanguage.English:
                return EnglishCulture;
            default:
                if (systemCulture.TwoLetterISOLanguageName == "de")
                {
                    return GermanCulture;
                }

                return EnglishCulture;
        }
    }
}
