using System.Collections.Immutable;
using Siteswaps.Generator.Generator.Filter.Combinatorics;
using Siteswaps.Generator.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Generator.Filter;

internal class FilterFactory
{
    public FilterFactory(SiteswapGeneratorInput input)
    {
        Input = input;
    }

    private SiteswapGeneratorInput Input { get; }

    public ISiteswapFilter Standard()
    {
        return new RightAmountOfBallsFilter(Input);
    }

    public ISiteswapFilter MinimumOccurenceFilter(int number, int amount)
    {
        return new AtLeastXXXTimesFilter(number, amount);
    }

    public ISiteswapFilter MaximumOccurenceFilter(int number, int amount)
    {
        return new AtMostXXXTimesFilter(number, amount);
    }

    public ISiteswapFilter ExactOccurenceFilter(int number, int amount)
    {
        return new ExactlyXXXTimesFilter(number, amount);
    }

    public ISiteswapFilter NoFilter()
    {
        return new NoFilter();
    }

    public ISiteswapFilter ExactNumberOfPassesFilter(int numberOfPasses, int numberOfJugglers)
    {
        return new NumberOfPassesFilter(numberOfPasses, numberOfJugglers, Input);
    }

    public ISiteswapFilter Combine(IEnumerable<ISiteswapFilter> filter)
    {
        return new AndFilter(filter.ToArray());
    }

    public ISiteswapFilter PatternFilter(IEnumerable<int> pattern, int numberOfJuggler)
    {
        return new PatternFilter(pattern.ToImmutableList(), numberOfJuggler, Input);
    }
    
    public ISiteswapFilter FlexiblePatternFilter(List<List<int>> pattern, int numberOfJuggler, bool isGlobalPattern)
    {
        return new FlexiblePatternFilter(pattern, numberOfJuggler, Input, isGlobalPattern);
    }

    public ISiteswapFilter GeneratePatternFilterHeuristics(IEnumerable<int> pattern, int numberOfJuggler) => new PatternFilterHeuristicBuilder(this).Build(pattern, numberOfJuggler, Input);

    public ISiteswapFilter OrFilter(ImmutableList<ISiteswapFilter> filter, ISiteswapFilter siteswapFilter) => new OrFilter(Combine(filter), siteswapFilter);

}