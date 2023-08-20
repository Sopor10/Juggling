using Shared;

namespace Siteswaps.Generator.Generator;

public record Siteswap
{
    public CyclicArray<sbyte> Values => this.Items.Select(x => (sbyte) x).ToCyclicArray().Rotate(this.Rotation);

    private int[] Items { get; }
    private int Rotation { get; init; } = 0;

    private Siteswap(int[] items)
    {
        this.Items = items;
    }


    public override string ToString()
    {
        return this.Values.EnumerateValues(1).ToSiteswapString();
    }

    public virtual bool Equals(Siteswap? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.ToString().Equals(other.ToString());
    }

    public override int GetHashCode()
    {
        return this.ToString().GetHashCode();
    }


    public static Siteswap CreateFromCorrect(params sbyte[] partialSiteswapItems)
    {
        return new Siteswap(partialSiteswapItems.Select(x => (int) x).ToArray());
    }


    public int Average => (int) this.Items.Average();

    public LocalSiteswap GetLocalSiteswap(int juggler, int numberOfJugglers, string? name = null)
    {
        return new LocalSiteswap(this, numberOfJugglers, juggler, name);
    }
    
    public IEnumerable<LocalSiteswap> GetLocalSiteswaps(int numberOfJugglers)
    {
        for (int i = 0; i < numberOfJugglers; i++)
        {
            yield return GetLocalSiteswap(i, numberOfJugglers);
        }
    }

    public Period Period => new(this.Values.Length);

    public LocalPeriod LocalPeriod(int numberOfJugglers)
    {
        return this.Period.GetLocalPeriod(numberOfJugglers);
    }


    public Siteswap? RotateToMatchInterface(Pattern @interface)
    {
        for (var i = 0; i < this.Interface.Values.Length; i++)
        {
            var rotate = this.Interface.Values.Rotate(i);
            if (@interface.Matches(rotate))
            {
                return this with {Rotation = this.Rotation + i % this.Interface.Values.Length};
            }
        }

        return null;
    }
    
    public Interface Interface => Interface.From(this);
}
