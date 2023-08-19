namespace Siteswaps.Generator.Generator;

using Shared;

public record LocalSiteswap
{
    private readonly string? name;

    public LocalSiteswap(Siteswap Siteswap, int NumberOfJugglers, int Juggler, string? name = null)
    {
        this.name = name;
        this.Siteswap = Siteswap;
        this.NumberOfJugglers = NumberOfJugglers;
        this.Juggler = Juggler;
        this.Values = this.Items().ToCyclicArray();
    }
    
    public string DisplayName => this.name ?? ((char)('A' + this.Juggler)).ToString();

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

    public (int Juggler, Hand Hand) GetThrowType(int position)
    {
        var juggler = Values[position] % NumberOfJugglers;
        var handArray = new CyclicArray<Hand>(Enumerable.Repeat(Hand.Right, NumberOfJugglers)
            .Concat(Enumerable.Repeat(Hand.Left, NumberOfJugglers)));

        var hand = handArray[Values[position] + position * NumberOfJugglers + Juggler];
        return (juggler, hand);
    }
    
    
    public void Deconstruct(out Siteswap Siteswap, out int NumberOfJugglers, out int Juggler)
    {
        Siteswap = this.Siteswap;
        NumberOfJugglers = this.NumberOfJugglers;
        Juggler = this.Juggler;
    }
}

public enum Hand
{
    Left,Right
}
