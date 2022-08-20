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
        var sum = 0;

        foreach (int item in value.Items)
        {
            if (item > 0)
            {
                sum += item;
            }
        }

        
        return !value.IsFilled() || sum == _generatorInputNumberOfObjects * value.Items.Count;
    }
}