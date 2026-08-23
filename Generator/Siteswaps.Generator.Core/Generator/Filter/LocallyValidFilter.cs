namespace Siteswaps.Generator.Core.Generator.Filter;

public class LocallyValidFilter(int numberOfJugglers, int juggler) : ISiteswapFilter
{
    private int NumberOfJugglers { get; } = numberOfJugglers;
    private int Juggler { get; } = juggler;

    public bool CanFulfill(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        var localLength =
            value.Length % NumberOfJugglers == 0 ? value.Length / NumberOfJugglers : value.Length;
        Span<bool> landings =
            localLength <= 128 ? stackalloc bool[localLength] : new bool[localLength];

        for (var index = 0; index < localLength; index++)
        {
            var throwHeight = value.Items[Juggler + index * NumberOfJugglers];
            var landing = (throwHeight + index) % localLength;
            if (landings[landing])
            {
                return false;
            }

            landings[landing] = true;
        }

        return true;
    }

    public int Order => 2;
}
