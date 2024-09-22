
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


    public double Average => Items.Average();

    public string GetLocalSiteswap(int juggler, int numberOfJugglers)
    {
        List<int> result = GetLocalSiteswapReal(juggler, numberOfJugglers);

        return ToString(result);
    }

    public List<int> GetLocalSiteswapReal(int juggler, int numberOfJugglers)
    {
        var result = new List<int>();

        var siteswap = Items.ToCyclicArray();
        for (var i = 0; i < LocalPeriod(numberOfJugglers).Value; i++)
        {
            result.Add(siteswap[juggler + i * (numberOfJugglers)]);
        }

        return result;
    }

    public Period Period => new(Items.Length);
    private LocalPeriod LocalPeriod(int numberOfJugglers) => Period.GetLocalPeriod(numberOfJugglers);
    
}
