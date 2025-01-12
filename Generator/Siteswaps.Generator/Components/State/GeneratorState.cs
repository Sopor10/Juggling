using System.Collections.Immutable;
using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Components.State;

public record GeneratorState
{
    public bool IsExactNumber => Objects is ExactNumber;
    public int? NumberOfJugglers { get; init; } = 2;
    public Objects Objects { get; init; } = new ExactNumber();
    public Period Period { get; init; } = new(5);
    public int? MaxThrow { get; init; } = 10;
    public int? MinThrow { get; init; } = 2;
    public bool IsGenerating { get; init; } = false;

    public ImmutableList<Throw> Throws { get; init; } =
        new[]
            {
                Throw.EmptyHand,
                Throw.Zip,
                Throw.Zap,
                Throw.Self,
                Throw.SinglePass,
                Throw.Heff,
                Throw.DoublePass,
                Throw.TripleSelf
            }
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

public record Throw(string Name, int Height, string DisplayValue)
{
    public static Throw AnyPass => new("Any Self", -3, "S");
    public static Throw AnySelf => new("Any Pass", -2, "P");
    public static Throw Empty => new("Empty", -1, "_");
    public static Throw EmptyHand => new("0", 0, "0");
    public static Throw Zip => new("Zip", 2, "Zip");
    public static Throw Hold => new("Hold", 4, "Hold");
    public static Throw Zap => new("Zap", 5, "Zap");
    public static Throw Self => new("Self", 6, "Self");
    public static Throw SinglePass => new("Single", 7, "Single");
    public static Throw Heff => new("Heff", 8, "Heff");
    public static Throw DoublePass => new("Double", 9, "Double");
    public static Throw TripleSelf => new("Triple S", 10, "Triple S");
    public static Throw TriplePass => new("Triple", 11, "Triple");

    private bool IsPass => Height % 2 == 1;

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

    public static IEnumerable<Throw> Everything => All.Concat(new List<Throw>
    {
        AnyPass,
        AnySelf,
        Empty
    });

    public IEnumerable<int> GetHeightForJugglers(int amountOfJugglers)
    {
        var result = new HashSet<int>();
        if (IsPass)
        {
            var min = Height - 1;
            var max = Height + 1;
            for (var i = min * amountOfJugglers + 1; i < max * amountOfJugglers; i++)
            {
                var item = i / 2;
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

    public static Throw Parse(string s)
    {
        return Everything.FirstOrDefault(x => x.DisplayValue == s) ?? throw new ArgumentException("Invalid throw");
    }
}
