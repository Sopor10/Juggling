using System.Collections.Immutable;
using Shared;

namespace Siteswaps.Generator.Generator.Filter;

/// <summary>
/// This filter checks the pattern only on filled siteswaps.
/// I don't know if it will also contain checks for min number of Passes or min number of throws.
/// I could parse the pattern and extract necessary filters. These would not be sufficient, but should speed up the generation
/// I don't know if this is placed correctly inside the PatternFilter or if it will be placed inside the FilterFactory
/// Depends on my future optimization for the FilterList 
/// </summary>
internal class PatternFilter : ISiteswapFilter
{
    private const int DontCare = -1;
    private const int Pass = -2;
    private const int Self = -3;
    
    private ImmutableList<int> Pattern { get; }
    private List<List<int>> Patterns { get; }

    private int NumberOfJuggler { get; }

    private HashSet<int> PassValues { get; }

    private HashSet<int> SelfValues { get; }
    
    public PatternFilter(ImmutableList<int> pattern, int numberOfJuggler, SiteswapGeneratorInput input)
    {
        Pattern = pattern;
        NumberOfJuggler = numberOfJuggler;
        Patterns = new List<List<int>>();

        for (var i = 0; i < pattern.Count; i++)
        {
            Patterns.Add(pattern.Rotate(i));
        }
        
        PassValues = Enumerable.Range(input.MinHeight, input.MaxHeight - input.MinHeight).Where(x => x % NumberOfJuggler != 0).ToHashSet();
        SelfValues = Enumerable.Range(input.MinHeight, input.MaxHeight - input.MinHeight).Where(x => x % NumberOfJuggler == 0).ToHashSet();

    }
    
    public bool CanFulfill(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        for (var i = 0; i < value.Items.Length; i++)
        {
            if (IsMatch(value, Patterns[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsMatch(PartialSiteswap value, List<int> pattern)
    {
        foreach (var (siteswapValue, patternValue) in value.Items.Zip(pattern))
        {
            if (patternValue == DontCare) continue;
            if (patternValue == Pass)
            {
                if (!PassValues.Contains(siteswapValue))
                {
                    return false;
                }
                continue;
            }
            if (patternValue == Self)
            {
                if (!SelfValues.Contains(siteswapValue))
                {
                    return false;
                }
                continue;
            }

            if (siteswapValue != patternValue) return false;
        }

        return true;
    }
}

public static class Extensions
{
    public static List<T> Rotate<T>(this IEnumerable<T> source, int number)
    {
        return source.ToCyclicArray().Rotate(number).EnumerateValues(1).ToList();
    }
}
