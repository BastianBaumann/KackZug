namespace KackZug.Core.Tests;

/// <summary>Tests, die die Sprache umstellen, laufen allein, damit andere Tests weiter Deutsch sehen</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class LanguageCollection
{
    public const string Name = "Language";
}
