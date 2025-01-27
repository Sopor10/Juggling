using System.Diagnostics;
using Shared;

namespace Siteswaps.Generator.Generator.Filter;

internal class RotationAwareFlexiblePatternFilter : ISiteswapFilter
{
    private List<List<int>> Pattern { get; }
    private int NumberOfJugglers { get; }
    private SiteswapGeneratorInput Input { get; }

    private HashSet<int> PassValues { get; }

    private HashSet<int> SelfValues { get; }

    private int Juggler { get; }

    public RotationAwareFlexiblePatternFilter(
        List<List<int>> pattern,
        int numberOfJugglers,
        SiteswapGeneratorInput input,
        int juggler
    )
    {
        Pattern = pattern;
        NumberOfJugglers = numberOfJugglers;
        Input = input;
        Juggler = juggler;

        PassValues = Enumerable
            .Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
            .Where(x => x % NumberOfJugglers != 0)
            .ToHashSet();
        SelfValues = Enumerable
            .Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
            .Where(x => x % NumberOfJugglers == 0)
            .ToHashSet();


    }

    public bool CanFulfill(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        var p = Enumerable.Repeat(new List<int> { -1 }, Input.Period).ToList();

        for (var i = 0; i < Pattern.Count; i++)
        {
            var pos = (Juggler + value.RotationIndex + i * NumberOfJugglers) % Input.Period;
            p[pos] = Pattern[i];
        }

        return new PatternRecord(p, SelfValues, PassValues).Matches(value.Items.ToCyclicArray());
    }

    [DebuggerDisplay("{DebugDisplay}")]
    private record PatternRecord(
        List<List<int>> Value,
        HashSet<int> SelfValues,
        HashSet<int> PassValues
    )
    {
        private string DebugDisplay =>
            string.Join(" ", Value.Select(x => "{" + string.Join(",", x) + "}"));

        private const int DontCare = -1;
        private const int Pass = -2;
        private const int Self = -3;

        public bool Matches(CyclicArray<int> value)
        {
            for (var i = 0; i < Value.Count; i++)
            {
                if (!RotationMatches(value, i))
                    return false;
            }

            return true;
        }

        private bool RotationMatches(CyclicArray<int> siteswap, int i)
        {
            var singleMatch = false;
            foreach (var patternValue in Value[i])
            {
                if (ValueSatisfiesPattern(siteswap[i], patternValue))
                {
                    singleMatch = true;
                }
            }

            return singleMatch;
        }

        private bool ValueSatisfiesPattern(int siteswapValue, int patternValue) =>
            patternValue switch
            {
                DontCare => true,
                Pass => PassValues.Contains(siteswapValue),
                Self => SelfValues.Contains(siteswapValue),
                _ => siteswapValue == patternValue,
            };
    }
}
