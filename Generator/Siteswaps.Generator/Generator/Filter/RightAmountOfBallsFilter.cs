using System.Diagnostics;

namespace Siteswaps.Generator.Generator.Filter;

internal class RightAmountOfBallsFilter(SiteswapGeneratorInput generatorInput) : ISiteswapFilter
{
    private readonly int _generatorInputNumberOfObjects = (int)generatorInput.NumberOfObjects;

    public bool CanFulfill(PartialSiteswap value)
    {
        if (!value.IsFilled())
        {
            return true;
        }

        return value.PartialSum == _generatorInputNumberOfObjects * value.Items.Length;
    }

    public int Order => 0;
}
