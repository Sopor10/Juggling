using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter;

internal class RightAmountOfBallsFilter : ISiteswapFilter
{
    private readonly sbyte _generatorInputNumberOfObjects;

    public RightAmountOfBallsFilter(SiteswapGeneratorInput generatorInput)
    {
        _generatorInputNumberOfObjects = (sbyte)generatorInput.NumberOfObjects;

    }


    public bool CanFulfill(IPartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }
        

        return value.PartialSum == _generatorInputNumberOfObjects * value.Items.Length;
    }
}