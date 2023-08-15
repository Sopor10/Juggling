namespace Siteswaps.Generator.Generator;

using Shared;

public record LocalSiteswap
{
    public LocalSiteswap(Siteswap Siteswap, int NumberOfJugglers, int Juggler)
    {
        this.Siteswap = Siteswap;
        this.NumberOfJugglers = NumberOfJugglers;
        this.Juggler = Juggler;
        this.Values = this.Items().ToCyclicArray();
    }

    public CyclicArray<int> Values { get; init; }

    public override string ToString() => this.Values.EnumerateValues(1).ToSiteswapString();

    private List<int> Items()
    {
        var result = new List<int>();

        var siteswap = this.Siteswap.Values;
        for (var i = 0; i < this.Siteswap.LocalPeriod(this.NumberOfJugglers).Value; i++)
        {
            result.Add(siteswap[this.Juggler + i * (this.NumberOfJugglers)]);
        }

        return result;
    }

    public LocalPeriod Period => this.Siteswap.LocalPeriod(this.NumberOfJugglers);
    public Siteswap Siteswap { get; init; }
    public int NumberOfJugglers { get; init; }
    public int Juggler { get; init; }

    public void Deconstruct(out Siteswap Siteswap, out int NumberOfJugglers, out int Juggler)
    {
        Siteswap = this.Siteswap;
        NumberOfJugglers = this.NumberOfJugglers;
        Juggler = this.Juggler;
    }
}