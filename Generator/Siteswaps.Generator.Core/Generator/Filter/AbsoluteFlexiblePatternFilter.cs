using System.Diagnostics;

namespace Siteswaps.Generator.Core.Generator.Filter;

/// <summary>
/// Matches a flexible Pass/Self/height pattern against absolute beat indices only
/// (no cyclic rotations, no juggler remapping).
/// </summary>
public class AbsoluteFlexiblePatternFilter : ISiteswapFilter
{
    private PatternRecord Pattern { get; }

    public AbsoluteFlexiblePatternFilter(
        List<List<int>> pattern,
        int numberOfJugglers,
        SiteswapGeneratorInput input
    )
    {
        var passValues = Enumerable
            .Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
            .Where(x => x % numberOfJugglers != 0)
            .ToHashSet();
        var selfValues = Enumerable
            .Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
            .Where(x => x % numberOfJugglers == 0)
            .ToHashSet();

        var slots = Enumerable.Repeat(new List<int> { -1 }, input.Period).ToList();
        for (var i = 0; i < pattern.Count && i < input.Period; i++)
        {
            slots[i] = pattern[i];
        }

        Pattern = new PatternRecord(slots, selfValues, passValues);
    }

    public bool CanFulfill(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        return Pattern.Matches(value.Items);
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
                if (!SlotMatches(value, i))
                {
                    return false;
                }
            }

            return true;
        }

        private bool SlotMatches(CyclicArray<int> siteswap, int i)
        {
            foreach (var patternValue in Value[i])
            {
                if (ValueSatisfiesPattern(siteswap[i], patternValue))
                {
                    return true;
                }
            }

            return false;
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

    public int Order => 10;
}
