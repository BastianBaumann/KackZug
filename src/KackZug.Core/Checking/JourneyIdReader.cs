namespace KackZug.Core.Checking;

/// <summary>Liest einzelne Angaben aus einer Fahrt-Kennung von bahn.de, etwa CA (Kategorie) oder ZE (Zugnummer)</summary>
public static class JourneyIdReader
{
    public static string? Read(string journeyId, string key)
    {
        var parts = journeyId.Split('#');
        for (var index = 0; index < parts.Length - 1; index++)
        {
            if (parts[index] == key)
            {
                var value = parts[index + 1].Trim();
                if (value.Length == 0)
                {
                    return null;
                }

                return value;
            }
        }

        return null;
    }
}
