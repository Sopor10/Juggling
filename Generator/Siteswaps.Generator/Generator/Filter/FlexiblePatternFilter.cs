using System.Diagnostics;
using Shared;

namespace Siteswaps.Generator.Generator.Filter;

internal class FlexiblePatternFilter : ISiteswapFilter
{

    //each position in a pattern can have multiple possible values
    private Pattern Pattern { get; }
    private List<Pattern> Patterns { get; }

    private int NumberOfJuggler { get; }

    private HashSet<int> PassValues { get; }

    private HashSet<int> SelfValues { get; }

    public FlexiblePatternFilter(List<List<int>> pattern, int numberOfJuggler, SiteswapGeneratorInput input, bool isGlobalPattern)
    {

        NumberOfJuggler = numberOfJuggler;
        PassValues = Enumerable.Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
            .Where(x => x % NumberOfJuggler != 0).ToHashSet();
        SelfValues = Enumerable.Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
            .Where(x => x % NumberOfJuggler == 0).ToHashSet();

        var p = Enumerable.Repeat(new List<int>{-1}, input.Period).ToList();

        for (var i = 0; i < pattern.Count; i++)
        {
            var pos = isGlobalPattern ? i :i * numberOfJuggler % input.Period;
            p[pos] = pattern[i];
        }
        
        Pattern = new Pattern(p, SelfValues, PassValues);
        Patterns = new List<Pattern>();

        for (var i = 0; i < input.Period; i++)
        {
            var rotate = new Pattern(p.Rotate(i), SelfValues, PassValues);
            Patterns.Add(rotate);
        }
    }

    public bool CanFulfill(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        var siteswap = value.Items.ToCyclicArray();
        return Patterns.Any(pattern => pattern.Matches(siteswap));
    }
}

[DebuggerDisplay("{DebugDisplay}")]
public record Pattern(List<List<int>> Value, HashSet<int> SelfValues, HashSet<int> PassValues)
{
    private string DebugDisplay => string.Join(" ", Value.Select(x => "{" + string.Join(",", x) + "}"));

    private const int DontCare = -1;
    private const int Pass = -2;
    private const int Self = -3;

    public bool Matches(CyclicArray<int> value)
    {
        for (var i = 0; i < Value.Count; i++)
        {
            if (!RotationMatches(value, i)) return false;
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
            _ => siteswapValue == patternValue
        };
}
