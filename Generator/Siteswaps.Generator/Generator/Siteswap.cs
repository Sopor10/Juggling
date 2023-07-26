
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
            _ => Convert.ToChar(i + 87).ToString()
        };
    }

    public virtual bool Equals(Siteswap? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ToString(Items).Equals(other.ToString(other.Items));
    }

    public override int GetHashCode()
    {
        return ToString(Items).GetHashCode();
    }


    public static Siteswap CreateFromCorrect(params sbyte[] partialSiteswapItems)
    {
        return new Siteswap(partialSiteswapItems.Select(x => (int)x).ToArray());
    }


    public int Average => (int)Items.Average();

    public string GetLocalSiteswap(int juggler, int numberOfJugglers)
    {
        var localPeriod = LocalPeriod(numberOfJugglers);
        var result = new List<int>();

        var siteswap = Items.ToCyclicArray();
        for (var i = 0; i < localPeriod; i++)
        {
            result.Add(siteswap[juggler + i * (numberOfJugglers)]);
        }

        return ToString(result);
    }

    private int Period => Items.Length;
    private int LocalPeriod(int numberOfJugglers) => Period % numberOfJugglers == 0 ? Period/numberOfJugglers : Period;
}