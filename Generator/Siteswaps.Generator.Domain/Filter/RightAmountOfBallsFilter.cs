using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter;

internal class RightAmountOfBallsFilter : ISiteswapFilter
{
    public RightAmountOfBallsFilter(SiteswapGeneratorInput generatorInput)
    {
        GeneratorInput = generatorInput;
    }

    private SiteswapGeneratorInput GeneratorInput { get; }

    public bool CanFulfill(IPartialSiteswap value) => !value.IsFilled() || Math.Abs(value.Items.Average() - GeneratorInput.NumberOfObjects) < 0.001;
}