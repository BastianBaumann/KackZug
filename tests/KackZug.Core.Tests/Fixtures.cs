namespace KackZug.Core.Tests;

/// <summary>Liest erfundene Beispielantworten aus dem Ordner Fixtures</summary>
public static class Fixtures
{
    public static string Read(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
        return File.ReadAllText(path);
    }
}
