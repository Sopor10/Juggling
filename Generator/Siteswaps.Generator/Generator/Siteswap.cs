using System.Diagnostics;

namespace Siteswaps.Generator.Generator;

[DebuggerDisplay("{ToString()}}")]
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

    public static Siteswap CreateFromCorrect(params int[] partialSiteswapItems) =>
        new(partialSiteswapItems.Select(x => (int)x).ToArray());

    public static Siteswap CreateFromCorrect(string s)
    {
        var result = new List<int>(s.Length);
        foreach (var i in s)
        {
            result.Add(
                i switch
                {
                    '0' => 0,
                    '1' => 1,
                    '2' => 2,
                    '3' => 3,
                    '4' => 4,
                    '5' => 5,
                    '6' => 6,
                    '7' => 7,
                    '8' => 8,
                    '9' => 9,
                    var x => x - 87,
                }
            );
        }
        Console.WriteLine(string.Join(',', result));
        return new(result.ToArray());
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
