using System.Collections.Immutable;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;
using Siteswaps.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Filter;

public class FilterFactory : IFilterFactory
{
    public SiteswapGeneratorInput Input { get; }

    public FilterFactory(SiteswapGeneratorInput input)
    {
        Input = input;
    }
    
    internal ISiteswapFilter Standard() => new FilterList(new CollisionFilter(), new AverageToHighFilter(Input), new AverageToLowFilter(Input), new RightAmountOfBallsFilter(Input));

    public ISiteswapFilter MinimumOccurenceFilter(int number, int amount) => new AtLeastXXXTimesFilter(number, amount);
    public ISiteswapFilter MaximumOccurenceFilter(int number, int amount) => new AtMostXXXTimesFilter(number, amount);
    public ISiteswapFilter ExactOccurenceFilter(int number, int amount) => new ExactlyXXXTimesFilter(number, amount);
    public ISiteswapFilter NoFilter() => new NoFilter();
    public ISiteswapFilter CollisionFilter() => new CollisionFilter();
    public ISiteswapFilter AverageToHighFilter() => new AverageToHighFilter(Input);
    public ISiteswapFilter AverageToLowFilter() => new AverageToLowFilter(Input);
    public ISiteswapFilter RightAmountOfBallsFilter() => new RightAmountOfBallsFilter(Input);
    public ISiteswapFilter ExactNumberOfPassesFilter(int numberOfPasses, int numberOfJugglers) => new NumberOfPassesFilter(numberOfPasses,numberOfJugglers,Input);
    public ISiteswapFilter Combine(IEnumerable<ISiteswapFilter> filter) => new FilterList(filter.ToArray());

    public ISiteswapFilter PatternFilter(IEnumerable<int> pattern) => new PatternFilter(pattern.ToImmutableList());
}
