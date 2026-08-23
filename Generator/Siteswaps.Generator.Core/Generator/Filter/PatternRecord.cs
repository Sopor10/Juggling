using System.Diagnostics;

namespace Siteswaps.Generator.Core.Generator.Filter;

[DebuggerDisplay("{DebugDisplay}")]
internal sealed record PatternRecord(
    List<List<int>> Value,
    NumberMask SelfValues,
    NumberMask PassValues
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
            var patternValues = Value[i];
            if (patternValues.Count == 1 && patternValues[0] == DontCare)
                continue;
            if (!RotationMatches(value, patternValues, i))
                return false;
        }
        return true;
    }

    private bool RotationMatches(CyclicArray<int> siteswap, List<int> patternValues, int i)
    {
        var singleMatch = false;
        foreach (var patternValue in patternValues)
            if (ValueSatisfiesPattern(siteswap[i], patternValue))
                singleMatch = true;
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
