using System.Collections.Immutable;

namespace Siteswaps.Generator.Components.State;

public record GeneratorState
{
    public bool IsExactNumber => Objects is ExactNumber;
    public int? NumberOfJugglers { get; init; } = 2;
    public Objects Objects { get; init; } = new ExactNumber();
    public int? Period { get; init; } = 5;
    public int? MaxThrow { get; init; } = 10;
    public int? MinThrow { get; init; } = 2;
    public bool IsGenerating { get; init; } = false;

    public ImmutableList<Throw> Throws { get; init; } =
        new[] { Throw.Zip,Throw.Hold, Throw.Zap, Throw.Self, Throw.SinglePass, Throw.Heff, Throw.DoublePass, Throw.TripleSelf  }
            .ToImmutableList();



    public ImmutableList<IFilterInformation> Filter { get; init; } = ImmutableList<IFilterInformation>.Empty;
    public bool CreateFilterFromThrowList { get; init; } = false;
}

public abstract record Objects;

public record ExactNumber : Objects
{
    public int? Number { get; init; } = 7;
}

public record Between : Objects
{
    public int? MinNumber { get; init; } = 6;
    public int? MaxNumber { get; init; } = 7;
}



public record Throw(string Name, int Height)
{
    public static Throw EmptyHand => new("Empty Hand", 0);
    public static Throw Zip => new("Zip", 2);
    public static Throw Hold => new("Hold", 4);
    public static Throw Zap => new("Zap", 5);
    public static Throw Self => new("Self", 6);
    public static Throw SinglePass => new("Single Pass", 7);
    public static Throw Heff => new("Heff", 8);
    public static Throw DoublePass => new("Double Pass", 9);
    public static Throw TripleSelf => new("Triple Self", 10);
    public static Throw TriplePass => new("Triple Pass", 11);

    public IEnumerable<int> GetHeightForJugglers(int amountOfJugglers)
    {
        var result = new HashSet<int>();
        if (IsSelf)
        {
            var min = Height - 1;
            var max = Height + 1;
            for (var i = (min * amountOfJugglers) + 1; i < max * amountOfJugglers; i++)
            {
                var item = i/2;
                if (item % amountOfJugglers != 0)
                {
                    result.Add(item);
                }
            }
        }
        else
        {
            result.Add(Height * amountOfJugglers / 2);
        }

        return result;
    }

    private bool IsSelf => Height % 2 == 1;

    public static IEnumerable<Throw> All => new List<Throw>
    {
        EmptyHand,
        Zip,
        Hold,
        Zap,
        Self,
        SinglePass,
        Heff,
        DoublePass,
        TripleSelf,
        TriplePass
    };
}
