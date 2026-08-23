namespace Siteswaps.Generator.Core.Generator.Filter;

internal sealed class StatePatternFilter(
    Siteswaps.Generator.Core.Generator.SiteswapGeneratorInput generatorInput,
    StatePattern pattern
) : ISiteswapFilter
{
    private readonly bool isUnconstrained = pattern.Items.All(x => x is StateValue.DontCare);
    private readonly StateMatchCache cachedMatches = new();

    public bool CanFulfill(PartialSiteswap value)
    {
        if (isUnconstrained || !value.IsFilled())
        {
            return true;
        }

        return cachedMatches.IsPatternMatch(
            value,
            value.RotationIndex,
            generatorInput.MaxHeight,
            pattern
        );
    }

    public bool CanFulfillAnyRotation(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        return cachedMatches.AnyPatternRotation(value, generatorInput.MaxHeight, pattern);
    }

    public int Order => 5;
    public bool CanRejectPartial => false;
    public bool IsRotationAware => !isUnconstrained;
}
