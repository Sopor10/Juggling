namespace Siteswaps.Generator.Core.Generator.Filter;

internal sealed class StatePatternFilter(
    Siteswaps.Generator.Core.Generator.SiteswapGeneratorInput generatorInput,
    StatePattern pattern
) : ISiteswapFilter
{
    private readonly bool isUnconstrained = pattern.Items.All(x => x is StateValue.DontCare);
    private PartialSiteswap? cachedSiteswap;
    private int cachedMutationVersion = -1;
    private ulong cachedMatchMask;
    private bool[]? cachedMatches;

    public bool CanFulfill(PartialSiteswap value)
    {
        if (isUnconstrained || !value.IsFilled())
        {
            return true;
        }

        EnsureCachedMatches(value);
        return IsMatch(value.RotationIndex);
    }

    public bool CanFulfillAnyRotation(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        EnsureCachedMatches(value);
        return cachedMatches is null
            ? cachedMatchMask != 0
            : Array.Exists(cachedMatches, match => match);
    }

    private void EnsureCachedMatches(PartialSiteswap value)
    {
        if (
            ReferenceEquals(cachedSiteswap, value)
            && cachedMutationVersion == value.MutationVersion
        )
        {
            return;
        }

        var stateValue = State.CalculateStateValue(value, generatorInput.MaxHeight);
        ulong matchMask = 0;
        bool[]? matches = value.Length > sizeof(ulong) * 8 ? new bool[value.Length] : null;
        for (var rotation = 0; rotation < value.Length; rotation++)
        {
            var matchesState = pattern.Matches(stateValue);
            if (matches is null)
            {
                if (matchesState)
                    matchMask |= 1UL << rotation;
            }
            else
            {
                matches[rotation] = matchesState;
            }
            stateValue = State.Advance(stateValue, value.Items[rotation]);
        }

        cachedSiteswap = value;
        cachedMutationVersion = value.MutationVersion;
        cachedMatchMask = matchMask;
        cachedMatches = matches;
    }

    private bool IsMatch(int rotation) =>
        cachedMatches is null
            ? (cachedMatchMask & (1UL << rotation)) != 0
            : cachedMatches[rotation];

    public int Order => 5;
    public bool CanRejectPartial => false;
    public bool IsRotationAware => !isUnconstrained;
}
