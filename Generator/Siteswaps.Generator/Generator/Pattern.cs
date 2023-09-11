namespace Siteswaps.Generator.Generator;

using System.Diagnostics;
using Shared;

[DebuggerDisplay("{DebugDisplay}")]
public record Pattern(List<List<int>> Value, HashSet<int> SelfValues, HashSet<int> PassValues)
{

    public static Pattern FromThrows(IEnumerable<int> throws, int numberOfJugglers)
    {
        return new Pattern(throws.Select(x => new List<int> {x}).ToList(),
            Throw.SelfValues(0,numberOfJugglers * 20, numberOfJugglers),
            Throw.PassValues(0,numberOfJugglers * 20, numberOfJugglers));
    }
    public static Pattern FromThrows(IEnumerable<Throw> throws, int numberOfJugglers)
    {
        var pattern = new List<List<int>>();
        foreach (var t in throws)
        {
            var heights = t.Height switch
            {
                -1 => new List<int> { -1 },
                -2 => new List<int> { -2 },
                -3 => new List<int> { -3 },

                _ => t.GetHeightForJugglers(numberOfJugglers).ToList()
            };
            pattern.Add(heights);
        }
        return new Pattern(pattern, Throw.SelfValues(0,numberOfJugglers * 20, numberOfJugglers), Throw.PassValues(0,numberOfJugglers * 20, numberOfJugglers));
    }
    
    private string DebugDisplay => string.Join(" ", Value.Select(x => "{" + string.Join(",", x.Select(y => y.Transform())) + "}"));

    private const int DontCare = -1;
    private const int Pass = -2;
    private const int Self = -3;

    public bool Matches(CyclicArray<sbyte> value)
    {
        for (var i = 0; i < Value.Count; i++)
        {
            if (this.PositionMatchesAnyAllowedThrow(i, value) is false)
            {
                return false;
            }
        }

        return true;
    }

    private bool PositionMatchesAnyAllowedThrow(int i, CyclicArray<sbyte> siteswap)
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
