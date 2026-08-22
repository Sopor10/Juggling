namespace Siteswaps.Generator.Core.Generator.Filter;

internal sealed class StatePatternFilter(
    Siteswaps.Generator.Core.Generator.SiteswapGeneratorInput generatorInput,
    StatePattern pattern
) : ISiteswapFilter
{
    private readonly bool isUnconstrained = pattern.Items.All(x => x is StateValue.DontCare);

    public bool CanFulfill(PartialSiteswap value)
    {
        if (isUnconstrained || !value.IsFilled())
        {
            return true;
        }

        return pattern.Matches(State.CalculateState(value, generatorInput.MaxHeight));
    }

    public int Order => 5;
    public bool IsRotationAware => !isUnconstrained;
}
