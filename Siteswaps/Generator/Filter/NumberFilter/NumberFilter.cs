using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter.NumberFilter;

internal abstract class NumberFilter : ISiteswapFilter
{
    public NumberFilter(int number, int amount)
    {
        Number = number;
        Amount = amount;
    }
    public bool CanFulfill(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput)
    {
        return CanFulfillNumberFilter(value, siteswapGeneratorInput);
    }

    private protected abstract bool CanFulfillNumberFilter(PartialSiteswap value, SiteswapGeneratorInput siteswapGeneratorInput);
    public int Number { get; }
    public int Amount { get; }
}