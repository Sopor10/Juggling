namespace Siteswaps.Generator.Core.Generator.Filter;

internal sealed class StateMatchCache
{
    private PartialSiteswap? siteswap;
    private int mutationVersion = -1;
    private ulong matchMask;
    private bool[]? matches;

    public bool IsMatch(PartialSiteswap value, int rotation, int maxHeight, uint expectedState)
    {
        Ensure(value, maxHeight, expectedState, static (state, expected) => state == expected);
        return IsMatch(rotation);
    }

    public bool IsPatternMatch(
        PartialSiteswap value,
        int rotation,
        int maxHeight,
        StatePattern pattern
    )
    {
        Ensure(value, maxHeight, pattern, static (state, expected) => expected.Matches(state));
        return IsMatch(rotation);
    }

    public bool AnyRotation(PartialSiteswap value, int maxHeight, uint expectedState)
    {
        Ensure(value, maxHeight, expectedState, static (state, expected) => state == expected);
        return HasAnyMatch();
    }

    public bool AnyPatternRotation(PartialSiteswap value, int maxHeight, StatePattern pattern)
    {
        Ensure(value, maxHeight, pattern, static (state, expected) => expected.Matches(state));
        return HasAnyMatch();
    }

    private void Ensure<T>(
        PartialSiteswap value,
        int maxHeight,
        T matcher,
        Func<uint, T, bool> stateMatches
    )
    {
        if (ReferenceEquals(siteswap, value) && mutationVersion == value.MutationVersion)
            return;

        var stateValue = State.CalculateStateValue(value, maxHeight);
        ulong newMatchMask = 0;
        bool[]? newMatches = value.Length > sizeof(ulong) * 8 ? new bool[value.Length] : null;
        for (var rotation = 0; rotation < value.Length; rotation++)
        {
            var matchesState = stateMatches(stateValue, matcher);
            if (newMatches is null)
            {
                if (matchesState)
                    newMatchMask |= 1UL << rotation;
            }
            else
            {
                newMatches[rotation] = matchesState;
            }
            stateValue = State.Advance(stateValue, value.Items[rotation]);
        }

        siteswap = value;
        mutationVersion = value.MutationVersion;
        matchMask = newMatchMask;
        matches = newMatches;
    }

    private bool IsMatch(int rotation) =>
        matches is null ? (matchMask & (1UL << rotation)) != 0 : matches[rotation];

    private bool HasAnyMatch() =>
        matches is null ? matchMask != 0 : Array.Exists(matches, match => match);
}
