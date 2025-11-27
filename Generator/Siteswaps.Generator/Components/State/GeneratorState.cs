using System.Collections.Immutable;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.State;

public record GeneratorState
{
    public int? NumberOfJugglers { get; init; } = 2;
    public Between Clubs { get; init; } = new();
    public Period Period { get; init; } = new(5);
    public int? MaxThrow { get; init; } = 10;
    public int? MinThrow { get; init; } = 2;

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
            Throw.TripleSelf,
        }.ToImmutableList();

    public FilterTree FilterTree { get; init; } = new(new AndNode());

    public bool CreateFilterFromThrowList => true;
    public Settings.SettingsDto Settings { get; set; } = new();
}

public record Between
{
    public int MinNumber { get; init; } = 6;
    public int MaxNumber { get; init; } = 6;
}

public record Throw(string Name, int Height, string DisplayValue)
{
    public static Throw AnyPass => new("Any Self", -3, "S");
    public static Throw AnySelf => new("Any Pass", -2, "P");
    public static Throw Empty => new("Empty", -1, "_");
    public static Throw EmptyHand => new("0", 0, "0");
    public static Throw Zip => new("Zip", 2, "Zip");
    public static Throw Three => new("3", 3, "3");
    public static Throw Hold => new("Hold", 4, "Hold");
    public static Throw Zap => new("Zap", 5, "Zap");
    public static Throw Self => new("Self", 6, "Self");
    public static Throw SinglePass => new("Single", 7, "Single");
    public static Throw Heff => new("Heff", 8, "Heff");
    public static Throw DoublePass => new("Double", 9, "Double");
    public static Throw TripleSelf => new("Triple S", 10, "Triple S");
    public static Throw TriplePass => new("Triple", 11, "Triple");
    public static Throw Quad => new("Quad", 12, "Quad");
    public static Throw QuadPass => new("Quad Pass", 13, "Quad Pass");

    private bool IsPass => Height % 2 == 1;

    private static IEnumerable<Throw> NamedThrows =>
        new List<Throw>
        {
            EmptyHand,
            Zip,
            Three,
            Hold,
            Zap,
            Self,
            SinglePass,
            Heff,
            DoublePass,
            TripleSelf,
            TriplePass,
            Quad,
            QuadPass,
        };

    public static IEnumerable<Throw> All(int height = 13)
    {
        foreach (var i in Enumerable.Range(0, height + 1))
        {
            if (NamedThrows.FirstOrDefault(x => x.Height == i) is { } throwItem)
            {
                yield return throwItem;
            }
            else
            {
                yield return new Throw(i.ToString(), i, i.ToString());
            }
        }
    }

    public static IEnumerable<Throw> Everything(int height = 13) =>
        All(height).Concat(AllWildCards);

    public static IEnumerable<Throw> AllWildCards => new List<Throw> { Empty, AnyPass, AnySelf };

    public IEnumerable<int> GetHeightForJugglers(int amountOfJugglers, bool useLiteralValue)
    {
        if (useLiteralValue)
        {
            return [Height];
        }

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

    public string GetDisplayValue(bool showName)
    {
        if (showName)
        {
            return DisplayValue;
        }
        return Height.ToString();
    }

    public static Throw Parse(string s)
    {
        return Everything().FirstOrDefault(x => x.DisplayValue == s)
            ?? throw new ArgumentException("Invalid throw");
    }
}
