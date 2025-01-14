using Shared;

namespace Siteswaps.Generator.Generator;

public record Siteswap
{
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

    public static Siteswap CreateFromCorrect(params int[] partialSiteswapItems)
    {
        return new Siteswap(partialSiteswapItems.Select(x => (int)x).ToArray());
    }

    public double Average => Items.Average();

    public LocalSiteswap GetLocalSiteswap(int juggler, int numberOfJugglers)
    {
        return new LocalSiteswap(this, juggler, numberOfJugglers);
    }

    public Period Period => new(Items.Length);
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
}
