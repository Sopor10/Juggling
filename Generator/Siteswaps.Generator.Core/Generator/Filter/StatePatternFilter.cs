namespace Siteswaps.Generator.Core.Generator.Filter;

internal sealed class StatePatternFilter(
    Siteswaps.Generator.Core.Generator.SiteswapGeneratorInput generatorInput,
    StatePattern pattern
) : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        return pattern.Matches(State.CalculateState(value, generatorInput.MaxHeight));
    }

    public int Order => 5;
    public bool IsRotationAware => true;
}
