namespace Siteswaps.Generator.Generator;

using global::Siteswap.Details.StateDiagram;
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
    
    public string JugglerName => this.name ?? ((char)('A' + this.Juggler)).ToString();

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
    public CyclicArray<T> RotateToLocal<T>(CyclicArray<T> array)
    {
        
        var result = new List<T>();

        for (var i = 0; i < this.Siteswap.LocalPeriod(this.NumberOfJugglers).Value; i++)
        {
            result.Add(array[this.Juggler + i * (this.NumberOfJugglers)]);
        }

        return result.ToCyclicArray();
    }

    public LocalPeriod Period => this.Siteswap.LocalPeriod(this.NumberOfJugglers);
    public Siteswap Siteswap { get; }
    public int NumberOfJugglers { get; }
    public int Juggler { get; }
    public Interface Interface => Interface.From(this);

    public (int Left, int Right) ClubDistribution
    {
        get
        {
            var state = StateGenerator.CalculateState(new global::Siteswap.Details.Siteswap(this.Siteswap.Values.EnumerateValues(1).Select(x => (int)x).ToArray()));
            var positions = state.Positions.Reverse();
            var countRight = positions.Where((_, i) => (i - this.Juggler) % 4 == 0).Count(x => x);
            var countLeft = positions.Where((_, i) => (i + 2 - this.Juggler) % 4 == 0).Count(x => x);
            return (countLeft, countRight);
        }
    }

    public (int Juggler, Hand Hand) GetThrowType(int position)
    {
        var juggler = (this.Values[position] + this.Juggler) % this.NumberOfJugglers;

        var hand = this.handArray[this.Values[position] + position * this.NumberOfJugglers + this.Juggler];
        return (juggler, hand);
    }
}

public enum Hand
{
    Left,Right
}
