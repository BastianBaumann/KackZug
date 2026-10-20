namespace KackZug.Core.Planning;

/// <summary>Gleichbleibende Zahl aus einem Text, damit Alarme nach einem Neustart wiedererkannt werden</summary>
/// <remarks>string.GetHashCode ist pro Prozess zufaellig und deshalb hier ungeeignet.</remarks>
public static class StableHash
{
    public static int Of(string text)
    {
        unchecked
        {
            var hash = 2166136261u;
            foreach (var character in text)
            {
                hash ^= character;
                hash *= 16777619u;
            }

            return (int)(hash & 0x3FFFFFFF) + 1000;
        }
    }
}
