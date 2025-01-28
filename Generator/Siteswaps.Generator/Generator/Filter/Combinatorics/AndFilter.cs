using Radzen;
using Siteswaps.Generator.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Generator.Filter.Combinatorics;

internal class AndFilter : ISiteswapFilter
{
    private List<ISiteswapFilter> Filters { get; }

    public AndFilter(IEnumerable<ISiteswapFilter> filters)
    {
        Filters = filters.OrderBy(Order).ToList();
    }

    public AndFilter(params ISiteswapFilter?[] filter)
        : this(filter.WhereNotNull().AsEnumerable()) { }

    public bool CanFulfill(PartialSiteswap value)
    {
        foreach (var filter in Filters)
        {
            if (filter.CanFulfill(value) is false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// lower is more restricitive and will therefore be checked first
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static int Order(ISiteswapFilter filter)
    {
        return filter switch
        {
            AndFilter _ => 0,
            OrFilter _ => 0,
            FlexiblePatternFilter _ => 10,
            RotationAwareFlexiblePatternFilter _ => 10,
            NoFilter _ => 0,
            AtLeastXXXTimesFilter _ => 0,
            AtMostXXXTimesFilter _ => 0,
            ExactlyXXXTimesFilter _ => 0,
            NumberFilter.NumberFilter _ => 0,
            NumberOfPassesFilter _ => 0,
            RightAmountOfBallsFilter _ => 0,
            NotFilter _ => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(filter)),
        };
    }
}

public class NotFilter(ISiteswapFilter filter) : ISiteswapFilter
{
    public bool CanFulfill(PartialSiteswap value)
    {
        if (value.IsFilled() is false)
        {
            return true;
        }

        return filter.CanFulfill(value) is false;
    }
}
