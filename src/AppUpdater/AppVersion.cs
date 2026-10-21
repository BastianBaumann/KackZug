namespace AppUpdater;

/// <summary>
/// A semantic-version-like app version such as "1.2.3" or "v1.2.3-dev.4".
/// A pre-release is lower than the release with the same core version; build metadata after "+" is ignored.
/// </summary>
public sealed class AppVersion : IComparable<AppVersion>, IEquatable<AppVersion>
{
    private readonly int[] _core;
    private readonly string[] _prerelease;

    private AppVersion(int[] core, string[] prerelease)
    {
        _core = core;
        _prerelease = prerelease;
    }

    public int Major
    {
        get { return _core[0]; }
    }

    public int Minor
    {
        get { return _core.Length > 1 ? _core[1] : 0; }
    }

    public int Patch
    {
        get { return _core.Length > 2 ? _core[2] : 0; }
    }

    public IReadOnlyList<int> CoreParts
    {
        get { return _core; }
    }

    /// <summary>The part after "-" without the dash, or an empty string for a release.</summary>
    public string Prerelease
    {
        get { return string.Join(".", _prerelease); }
    }

    public bool IsPrerelease
    {
        get { return _prerelease.Length > 0; }
    }

    public static AppVersion Parse(string text)
    {
        if (!TryParse(text, out var version))
        {
            throw new FormatException("Not a valid version: '" + text + "'");
        }

        return version;
    }

    public static bool TryParse(string? text, out AppVersion version)
    {
        version = null!;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var cleaned = text.Trim();
        if (cleaned.StartsWith('v') || cleaned.StartsWith('V'))
        {
            cleaned = cleaned.Substring(1);
        }

        var plus = cleaned.IndexOf('+');
        if (plus >= 0)
        {
            cleaned = cleaned.Substring(0, plus);
        }

        var corePart = cleaned;
        var prereleasePart = string.Empty;
        var dash = cleaned.IndexOf('-');
        if (dash >= 0)
        {
            corePart = cleaned.Substring(0, dash);
            prereleasePart = cleaned.Substring(dash + 1);
            if (prereleasePart.Length == 0)
            {
                return false;
            }
        }

        var coreTexts = corePart.Split('.');
        if (coreTexts.Length > 4)
        {
            return false;
        }

        var core = new int[coreTexts.Length];
        for (var i = 0; i < coreTexts.Length; i++)
        {
            if (!IsDigits(coreTexts[i]) || !int.TryParse(coreTexts[i], out core[i]))
            {
                return false;
            }
        }

        var prerelease = Array.Empty<string>();
        if (prereleasePart.Length > 0)
        {
            prerelease = prereleasePart.Split('.');
            foreach (var identifier in prerelease)
            {
                if (identifier.Length == 0 || !identifier.All(IsIdentifierChar))
                {
                    return false;
                }
            }
        }

        version = new AppVersion(core, prerelease);
        return true;
    }

    public int CompareTo(AppVersion? other)
    {
        if (other is null)
        {
            return 1;
        }

        var length = Math.Max(_core.Length, other._core.Length);
        for (var i = 0; i < length; i++)
        {
            var mine = i < _core.Length ? _core[i] : 0;
            var theirs = i < other._core.Length ? other._core[i] : 0;
            if (mine != theirs)
            {
                return mine.CompareTo(theirs);
            }
        }

        if (!IsPrerelease && !other.IsPrerelease)
        {
            return 0;
        }

        if (!IsPrerelease)
        {
            return 1;
        }

        if (!other.IsPrerelease)
        {
            return -1;
        }

        var count = Math.Min(_prerelease.Length, other._prerelease.Length);
        for (var i = 0; i < count; i++)
        {
            var result = CompareIdentifiers(_prerelease[i], other._prerelease[i]);
            if (result != 0)
            {
                return result;
            }
        }

        return _prerelease.Length.CompareTo(other._prerelease.Length);
    }

    public bool Equals(AppVersion? other)
    {
        return CompareTo(other) == 0;
    }

    public override bool Equals(object? obj)
    {
        return obj is AppVersion other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        var length = _core.Length;
        while (length > 1 && _core[length - 1] == 0)
        {
            length--;
        }

        for (var i = 0; i < length; i++)
        {
            hash.Add(_core[i]);
        }

        foreach (var identifier in _prerelease)
        {
            hash.Add(identifier, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }

    public override string ToString()
    {
        var text = string.Join(".", _core);
        if (IsPrerelease)
        {
            text += "-" + Prerelease;
        }

        return text;
    }

    public static bool operator >(AppVersion left, AppVersion right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator <(AppVersion left, AppVersion right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >=(AppVersion left, AppVersion right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <=(AppVersion left, AppVersion right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator ==(AppVersion? left, AppVersion? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    public static bool operator !=(AppVersion? left, AppVersion? right)
    {
        return !(left == right);
    }

    private static int CompareIdentifiers(string left, string right)
    {
        var leftIsNumber = IsDigits(left);
        var rightIsNumber = IsDigits(right);
        if (leftIsNumber && rightIsNumber)
        {
            var leftTrimmed = left.TrimStart('0');
            var rightTrimmed = right.TrimStart('0');
            if (leftTrimmed.Length != rightTrimmed.Length)
            {
                return leftTrimmed.Length.CompareTo(rightTrimmed.Length);
            }

            return string.CompareOrdinal(leftTrimmed, rightTrimmed);
        }

        if (leftIsNumber)
        {
            return -1;
        }

        if (rightIsNumber)
        {
            return 1;
        }

        return string.CompareOrdinal(left, right);
    }

    private static bool IsDigits(string text)
    {
        return text.Length > 0 && text.All(char.IsAsciiDigit);
    }

    private static bool IsIdentifierChar(char character)
    {
        return char.IsAsciiLetterOrDigit(character) || character == '-';
    }
}
