using Shared;

namespace Siteswaps.Generator.Generator.Filter;

internal class FlexiblePatternFilter : ISiteswapFilter
{
    private const int DontCare = -1;
    private const int Pass = -2;
    private const int Self = -3;

    //each position in a pattern can have multiple possible values
    private List<List<int>> Pattern { get; }
    private List<List<List<int>>> Patterns { get; }

    private int NumberOfJuggler { get; }
    public bool IsGlobalPattern { get; }

    private HashSet<int> PassValues { get; }

    private HashSet<int> SelfValues { get; }

    public FlexiblePatternFilter(List<List<int>> pattern, int numberOfJuggler, SiteswapGeneratorInput input, bool isGlobalPattern)
    {
        Pattern = pattern;
        NumberOfJuggler = numberOfJuggler;
        IsGlobalPattern = isGlobalPattern;
        Patterns = new List<List<List<int>>>();

        for (var i = 0; i < pattern.Count; i++)
        {
            var rotate = pattern.Rotate(i);
            Patterns.Add(rotate);
        }

        PassValues = Enumerable.Range(input.MinHeight, input.MaxHeight - input.MinHeight)
            .Where(x => x % NumberOfJuggler != 0).ToHashSet();
        SelfValues = Enumerable.Range(input.MinHeight, input.MaxHeight - input.MinHeight)
            .Where(x => x % NumberOfJuggler == 0).ToHashSet();

    }

    public bool CanFulfill(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        var siteswap = value.Items.ToCyclicArray();
        for (var i = 0; i < value.Items.Length; i++)
        {
            if (IsMatch(siteswap, Patterns[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsMatch(CyclicArray<sbyte> value, List<List<int>> pattern)
    {
        for (int i = 0; i < pattern.Count; i++)
        {
            var pos = Position(i);

            var singleMatch = false;
            foreach (var patternValue in pattern[i])
            {
                if (IsSingleMatch(value[pos], patternValue) )
                {
                    singleMatch = true;
                }
            }

            if (singleMatch is false)
            {
                return false;
            }
            
        }

        return true;
    }

    private int Position(int i) => IsGlobalPattern ? i : i * NumberOfJuggler;

    private bool IsSingleMatch(sbyte siteswapValue, int patternValue)
    {
        if (patternValue == DontCare) return true;
        if (patternValue == Pass)
        {
            if (!PassValues.Contains(siteswapValue))
            {
                return false;
            }

            return true;
        }

        if (patternValue == Self)
        {
            if (!SelfValues.Contains(siteswapValue))
            {
                return false;
            }

            return true;
        }

        if (siteswapValue != patternValue) return false;
        return true;
    }
}