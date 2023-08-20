namespace Siteswaps.Generator.Generator;

using Shared;

public record LocalSiteswap
{
    private readonly string? name;
    private CyclicArray<Hand> handArray;

    public LocalSiteswap(Siteswap Siteswap, int NumberOfJugglers, int Juggler, string? name = null)
    {
        this.name = name;
        this.Siteswap = Siteswap;
        this.NumberOfJugglers = NumberOfJugglers;
        this.Juggler = Juggler;
        this.Values = this.Items().ToCyclicArray();
        this.handArray = new CyclicArray<Hand>(Enumerable.Repeat(Hand.Right, this.NumberOfJugglers)
            .Concat(Enumerable.Repeat(Hand.Left, this.NumberOfJugglers)));
    }
    
    public string DisplayName => this.name ?? ((char)('A' + this.Juggler)).ToString();

    public CyclicArray<sbyte> Values { get; }

    public override string ToString() => this.Values.EnumerateValues(1).ToSiteswapString();

    private List<sbyte> Items()
    {
        var result = new List<sbyte>();

        var siteswap = this.Siteswap.Values;
        for (var i = 0; i < this.Siteswap.LocalPeriod(this.NumberOfJugglers).Value; i++)
        {
            result.Add(siteswap[this.Juggler + i * (this.NumberOfJugglers)]);
        }

        return result;
    }

    public LocalPeriod Period => this.Siteswap.LocalPeriod(this.NumberOfJugglers);
    public Siteswap Siteswap { get; }
    public int NumberOfJugglers { get; }
    public int Juggler { get; }
    public Interface Interface => Interface.From(this);

    public (int Juggler, Hand Hand) GetThrowType(int position)
    {
        var juggler = (Values[position] + Juggler) % NumberOfJugglers;

        var hand = this.handArray[Values[position] + position * NumberOfJugglers + Juggler];
        return (juggler, hand);
    }
}

public enum Hand
{
    Left,Right
}
