using System.Collections.Immutable;

namespace Siteswaps.Generator.Filter;

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
    public PatternFilter(ImmutableList<int> pattern)
    {
        Pattern = pattern;
    }
    
    public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        // TODO implement check on filled partial siteswap
        return true;
    }
}