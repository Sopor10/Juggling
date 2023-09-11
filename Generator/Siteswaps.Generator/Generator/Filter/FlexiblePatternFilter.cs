namespace Siteswaps.Generator.Generator.Filter;

using Shared;

internal class FlexiblePatternFilter : PatternFilter
{

    public FlexiblePatternFilter(Pattern pattern, int numberOfJuggler, SiteswapGeneratorInput input, bool isGlobalPattern) : base(pattern, numberOfJuggler, input, isGlobalPattern)
    {
    }

    protected override CyclicArray<sbyte> Matches(PartialSiteswap value) => value.Items.ToCyclicArray();
}
