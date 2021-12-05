using Siteswaps.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Filter;

public class FilterFactory
{
    public ISiteswapFilter Standard() => new FilterList(new CollisionFilter(), new AverageToHighFilter(), new AverageToLowFilter(), new RightAmountOfBallsFilter());

    public ISiteswapFilter MinimumOccurenceFilter(int number, int amount) => new AtLeastXXXTimesFilter(number, amount);
    public ISiteswapFilter MaximumOccurenceFilter(int number, int amount) => new AtMostXXXTimesFilter(number, amount);
    public ISiteswapFilter ExactOccurenceFilter(int number, int amount) => new ExactlyXXXTimesFilter(number, amount);

    public ISiteswapFilter NoFilter() => new NoFilter();
    public ISiteswapFilter CollisionFilter() => new CollisionFilter();
    public ISiteswapFilter AverageToHighFilter() => new AverageToHighFilter();
    public ISiteswapFilter AverageToLowFilter() => new AverageToLowFilter();
    public ISiteswapFilter RightAmountOfBallsFilter() => new RightAmountOfBallsFilter();
}