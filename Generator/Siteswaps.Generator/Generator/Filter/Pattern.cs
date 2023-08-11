namespace Siteswaps.Generator.Generator.Filter;

using System.Diagnostics;
using Shared;

[DebuggerDisplay("{DebugDisplay}")]
public record Pattern(List<List<int>> Value, HashSet<int> SelfValues, HashSet<int> PassValues)
{
    private string DebugDisplay => string.Join(" ", Value.Select(x => "{" + string.Join(",", x) + "}"));

    private const int DontCare = -1;
    private const int Pass = -2;
    private const int Self = -3;

    public bool Matches(CyclicArray<sbyte> value)
    {
        for (var i = 0; i < Value.Count; i++)
        {
            if (!this.RotationMatches(value, i)) return false;
        }

        return true;
    }

    private bool RotationMatches(CyclicArray<sbyte> siteswap, int i)
    {

        var singleMatch = false;
        foreach (var patternValue in Value[i])
        {
            if (this.ValueSatisfiesPattern(siteswap[i], patternValue))
            {
                singleMatch = true;
            }
        }

        return singleMatch;
    }
    

    private bool ValueSatisfiesPattern(sbyte siteswapValue, int patternValue) =>
        patternValue switch
        {
            DontCare => true,
            Pass => PassValues.Contains(siteswapValue),
            Self => SelfValues.Contains(siteswapValue),
            _ => siteswapValue == patternValue
        };
}