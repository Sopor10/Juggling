using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter.NumberFilter;

internal abstract class NumberFilter : ISiteswapFilter
{
    protected NumberFilter(int number, int amount)
    {
        Number = number;
        Amount = amount;
    }
    public bool CanFulfill(IPartialSiteswap value)
    {
        return CanFulfillNumberFilter(value);
    }

    private protected abstract bool CanFulfillNumberFilter(IPartialSiteswap value);
    protected int Number { get; }
    protected int Amount { get; }
}