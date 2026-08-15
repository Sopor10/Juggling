using System.Globalization;

namespace Siteswap.Details;

/// <summary>
/// Maps between named passing throws (Zip, Hold, Self, …) and global siteswap heights
/// depending on the number of jugglers. Base heights are the familiar two-juggler /
/// four-handed values; <see cref="HeightsFor"/> scales them for other juggler counts.
/// </summary>
public static class PassingThrowNames
{
    private sealed record NamedThrow(string Display, int BaseHeight);

    /// <summary>
    /// Catalog ordered from low to high base height. First match wins on reverse lookup.
    /// </summary>
    private static readonly NamedThrow[] Catalog =
    [
        new("0", 0),
        new("Zip", 2),
        new("3", 3),
        new("Hold", 4),
        new("Zap", 5),
        new("Self", 6),
        new("Single", 7),
        new("Heff", 8),
        new("Double", 9),
        new("Triple S", 10),
        new("Triple", 11),
        new("Quad", 12),
        new("Quad Pass", 13),
    ];

    /// <summary>
    /// Global heights that correspond to a named throw's base height for the given juggler count.
    /// </summary>
    public static IReadOnlyCollection<int> HeightsFor(int baseHeight, int numberOfJugglers)
    {
        var jugglers = Math.Max(1, numberOfJugglers);
        var result = new HashSet<int>();
        if (baseHeight % 2 == 1)
        {
            var min = baseHeight - 1;
            var max = baseHeight + 1;
            for (var i = min * jugglers + 1; i < max * jugglers; i++)
            {
                var item = i / 2;
                if (item % jugglers != 0)
                {
                    result.Add(item);
                }
            }
        }
        else
        {
            result.Add(baseHeight * jugglers / 2);
        }

        return result;
    }

    /// <summary>
    /// Display name for a global throw height at the given juggler count.
    /// Falls back to the local height when no named throw matches.
    /// </summary>
    public static string Format(int height, int numberOfJugglers)
    {
        var jugglers = Math.Max(1, numberOfJugglers);
        foreach (var named in Catalog)
        {
            if (HeightsFor(named.BaseHeight, jugglers).Contains(height))
            {
                return named.Display;
            }
        }

        return ToLocalDisplay(height, jugglers);
    }

    /// <summary>
    /// Local height for a global throw: <c>global / jugglers</c>, formatted like local notation.
    /// </summary>
    public static string ToLocalDisplay(int height, int numberOfJugglers)
    {
        var jugglers = Math.Max(1, numberOfJugglers);
        return (height * 1.0 / jugglers).ToString("0.##", CultureInfo.InvariantCulture);
    }
}
