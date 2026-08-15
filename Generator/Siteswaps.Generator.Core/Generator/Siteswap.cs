using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Siteswaps.Generator.Core.Generator;

[DebuggerDisplay("{ToString()}}")]
public record Siteswap
{
    /// <summary>
    /// Upper bound for period length when parsing user-supplied notation.
    /// </summary>
    public const int MaxPeriodLength = 64;

    public int[] Items { get; }

    private Siteswap(int[] items)
    {
        Items = items;
    }

    public override string ToString()
    {
        return ToString(Items);
    }

    private string ToString(IEnumerable<int> items)
    {
        return string.Join("", items.Select(Transform));
    }

    private string Transform(int i)
    {
        return i switch
        {
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString(),
        };
    }

    public virtual bool Equals(Siteswap? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return ToString(Items).Equals(other.ToString(other.Items));
    }

    public override int GetHashCode()
    {
        return ToString(Items).GetHashCode();
    }

    public static Siteswap CreateFromCorrect(params int[] partialSiteswapItems) =>
        new(partialSiteswapItems.Select(x => (int)x).ToArray());

    public static Siteswap CreateFromCorrect(string s)
    {
        if (!TryCreate(s, out var siteswap) || siteswap is null)
        {
            throw new ArgumentException("Invalid siteswap notation.", nameof(s));
        }

        return siteswap;
    }

    /// <summary>
    /// Parses siteswap notation (<c>0-9</c>, <c>a-z</c>/<c>A-Z</c> for heights ≥ 10).
    /// Rejects empty, overlong, non-notation characters, and landing-invalid patterns.
    /// </summary>
    public static bool TryCreate(string? s, [NotNullWhen(true)] out Siteswap? siteswap)
    {
        siteswap = null;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        s = s.Trim();
        if (s.Length is < 1 or > MaxPeriodLength)
        {
            return false;
        }

        var result = new int[s.Length];
        for (var i = 0; i < s.Length; i++)
        {
            if (!TryParseThrowHeight(s[i], out var height))
            {
                return false;
            }

            result[i] = height;
        }

        var created = new Siteswap(result);
        if (!created.IsValid())
        {
            return false;
        }

        siteswap = created;
        return true;
    }

    private static bool TryParseThrowHeight(char c, out int height)
    {
        switch (c)
        {
            case >= '0' and <= '9':
                height = c - '0';
                return true;
            case >= 'a' and <= 'z':
                height = c - 'a' + 10;
                return true;
            case >= 'A' and <= 'Z':
                height = c - 'A' + 10;
                return true;
            default:
                height = 0;
                return false;
        }
    }

    public double Average => Items.Average();

    public LocalSiteswap GetLocalSiteswap(int juggler, int numberOfJugglers)
    {
        return new LocalSiteswap(this, juggler, numberOfJugglers);
    }

    public Period Period => new(Items.Length);

    public bool IsValid() =>
        Items.Select((x, i) => (x + i) % Items.Length).ToHashSet().Count == Items.Length;
}

public record LocalSiteswap(Siteswap Siteswap, int Juggler, int NumberOfJugglers)
{
    public string GlobalNotation => ToString();
    public string LocalNotation =>
        string.Join(
            " ",
            GetLocalSiteswapReal()
                .Select(x => x * 1.0 / NumberOfJugglers)
                .Select(x => x.ToString("0.##"))
        );

    private List<int> GetLocalSiteswapReal()
    {
        var result = new List<int>();

        var siteswap = Siteswap.Items.ToCyclicArray();
        for (var i = 0; i < Siteswap.Period.GetLocalPeriod(NumberOfJugglers).Value; i++)
        {
            result.Add(siteswap[Juggler + i * NumberOfJugglers]);
        }

        return result;
    }

    public override string ToString()
    {
        return ToString(GetLocalSiteswapReal());
    }

    private string ToString(IEnumerable<int> items)
    {
        return string.Join("", items.Select(Transform));
    }

    private string Transform(int i)
    {
        return i switch
        {
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString(),
        };
    }

    public double Average()
    {
        return GetLocalSiteswapReal().Average() * 1.0 / NumberOfJugglers;
    }

    public bool IsValidAsGlobalSiteswap()
    {
        var items = GetLocalSiteswapReal();

        return items.Select((x, i) => (x + i) % items.Count).ToHashSet().Count == items.Count;
    }
}
