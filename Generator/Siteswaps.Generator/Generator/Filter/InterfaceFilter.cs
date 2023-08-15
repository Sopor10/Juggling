using Shared;

namespace Siteswaps.Generator.Generator.Filter;

internal class InterfaceFilter : PatternFilter
{

    public InterfaceFilter(Pattern pattern, int numberOfJuggler, SiteswapGeneratorInput input, bool isGlobalPattern) : base(pattern, numberOfJuggler, input, isGlobalPattern)
    {
    }

    protected override CyclicArray<sbyte> Matches(PartialSiteswap value) => value.Interface;
}
