using KackZug.Core.Models;

namespace KackZug.Core.Checking;

/// <summary>Sucht auf der Abfahrtstafel genau den gespeicherten Zug zur geplanten Zeit</summary>
public static class BoardMatcher
{
    public static BoardEntry? Find(IReadOnlyList<BoardEntry> entries, Leg leg, DateTime plannedDeparture)
    {
        var candidates = entries.Where(entry => entry.PlannedTime == plannedDeparture).ToList();
        if (candidates.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(leg.TrainNumber))
        {
            foreach (var candidate in candidates)
            {
                if (candidate.Number == leg.TrainNumber && CategoryFits(candidate, leg))
                {
                    return candidate;
                }
            }
        }

        if (!string.IsNullOrEmpty(leg.Name))
        {
            var sameName = candidates.Where(candidate => SameText(candidate.Name, leg.Name)).ToList();
            if (sameName.Count == 1)
            {
                return sameName[0];
            }
        }

        if (!string.IsNullOrEmpty(leg.Line))
        {
            var sameLine = candidates.Where(candidate => SameText(candidate.LineNumber, leg.Line) && CategoryFits(candidate, leg)).ToList();
            if (sameLine.Count == 1)
            {
                return sameLine[0];
            }
        }

        return null;
    }

    /// <remarks>Von Hand eingegeben steht oft "RE 1", auf der Tafel "RE1".</remarks>
    private static bool SameText(string? boardText, string? savedText)
    {
        if (string.IsNullOrEmpty(boardText) || string.IsNullOrEmpty(savedText))
        {
            return false;
        }

        return string.Equals(Compact(boardText), Compact(savedText), StringComparison.OrdinalIgnoreCase);
    }

    private static string Compact(string text)
    {
        return string.Concat(text.Where(character => !char.IsWhiteSpace(character)));
    }

    private static bool CategoryFits(BoardEntry candidate, Leg leg)
    {
        if (string.IsNullOrEmpty(candidate.Category) || string.IsNullOrEmpty(leg.Category))
        {
            return true;
        }

        return string.Equals(candidate.Category, leg.Category, StringComparison.OrdinalIgnoreCase);
    }
}
