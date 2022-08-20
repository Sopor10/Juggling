using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter;

internal class AverageToHighFilter : ISiteswapFilter
{
    private SiteswapGeneratorInput GeneratorInput { get; }

    public AverageToHighFilter(SiteswapGeneratorInput siteswapGeneratorInput)
    {
        this.GeneratorInput = siteswapGeneratorInput;
    }

    public bool CanFulfill(IPartialSiteswap value)
    {
        var minAdditionalValue =
            value.Items.Count(x => x == PartialSiteswap.Free) * GeneratorInput.MinHeight;

        var sum = value.Items.Where(x => x >= 0).Sum();

        var average = (sum * 1.0 + minAdditionalValue) / GeneratorInput.Period;
        return average <= GeneratorInput.NumberOfObjects;
    }
}