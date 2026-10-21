using System.Collections;
using System.Globalization;
using System.Resources;
using KackZug.Core.Localization;
using KackZug.Core.Models;

namespace KackZug.Core.Tests.Localization;

/// <summary>Prueft die Wahl der Sprache und dass beide Texttabellen dieselben Schluessel haben</summary>
public class LanguageTests
{
    [Theory]
    [InlineData("de-DE")]
    [InlineData("de-AT")]
    [InlineData("de-CH")]
    public void SystemWithGermanDeviceIsGerman(string systemCulture)
    {
        var culture = Language.Resolve(AppLanguage.System, new CultureInfo(systemCulture));

        Assert.Equal("de", culture.TwoLetterISOLanguageName);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("fr-FR")]
    public void SystemWithOtherDeviceIsEnglish(string systemCulture)
    {
        var culture = Language.Resolve(AppLanguage.System, new CultureInfo(systemCulture));

        Assert.Equal("en", culture.TwoLetterISOLanguageName);
    }

    [Fact]
    public void ExplicitChoiceWinsOverDevice()
    {
        var german = Language.Resolve(AppLanguage.German, new CultureInfo("fr-FR"));
        var english = Language.Resolve(AppLanguage.English, new CultureInfo("de-DE"));

        Assert.Equal("de", german.TwoLetterISOLanguageName);
        Assert.Equal("en", english.TwoLetterISOLanguageName);
    }

    [Fact]
    public void GermanAndEnglishHaveTheSameKeys()
    {
        var english = KeysOf(CultureInfo.InvariantCulture);
        var german = KeysOf(new CultureInfo("de"));

        Assert.NotEmpty(english);
        Assert.Equal(english, german);
    }

    [Fact]
    public void NoTextIsEmpty()
    {
        foreach (var cultureName in new[] { "", "de" })
        {
            var set = Strings.ResourceManager.GetResourceSet(new CultureInfo(cultureName), true, false)!;
            foreach (DictionaryEntry entry in set)
            {
                Assert.False(string.IsNullOrWhiteSpace(entry.Value as string), cultureName + ": " + entry.Key);
            }
        }
    }

    private static List<string> KeysOf(CultureInfo culture)
    {
        var set = Strings.ResourceManager.GetResourceSet(culture, true, false);
        Assert.NotNull(set);
        var keys = new List<string>();
        foreach (DictionaryEntry entry in set)
        {
            keys.Add((string)entry.Key);
        }

        keys.Sort(StringComparer.Ordinal);
        return keys;
    }
}
