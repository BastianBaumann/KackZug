using System.Runtime.CompilerServices;
using KackZug.Core.Localization;
using KackZug.Core.Models;

namespace KackZug.Core.Tests;

/// <summary>Alle Tests laufen auf Deutsch, unabhaengig von der Sprache des Rechners</summary>
public static class TestLanguage
{
    [ModuleInitializer]
    public static void UseGerman()
    {
        Language.Apply(AppLanguage.German);
    }
}
