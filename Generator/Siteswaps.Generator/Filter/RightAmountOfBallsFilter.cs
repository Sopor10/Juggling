using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter;

internal class RightAmountOfBallsFilter : ISiteswapFilter
{
    public RightAmountOfBallsFilter(SiteswapGeneratorInput generatorInput)
    {
        GeneratorInput = generatorInput;
    }

    private SiteswapGeneratorInput GeneratorInput { get; }

    public bool CanFulfill(IPartialSiteswap value)
    {
        if (!value.IsFilled()) return true;

        return Math.Abs(value.Items.Average() - GeneratorInput.NumberOfObjects) < 0.001;
    }
}