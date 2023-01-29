using Shared;

namespace Siteswaps.Generator.Generator;

public record Siteswap
{
    private int[] Items { get; }

    private Siteswap(int[] items)
    {
        Items = items;
    }


    public override string ToString()
    {
        return string.Join("", Items.Select(Transform));
    }

    private string Transform(int i)
    {
        return i switch
        {
            < 10 => $"{i}",
            _ => Convert.ToChar(i + 87).ToString()
        };
    }

    public virtual bool Equals(Siteswap? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ToString().Equals(other.ToString());
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }


    public static Siteswap CreateFromCorrect(sbyte[] partialSiteswapItems)
    {
        return new Siteswap(partialSiteswapItems.Select(x => (int)x).ToArray());
    }


    public int Average => (int)Items.Average();
}