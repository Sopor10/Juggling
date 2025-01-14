using Siteswaps.Generator.Generator.Filter.Combinatorics;
using Siteswaps.Generator.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Generator.Filter;

internal class PatternFilterHeuristicBuilder
{
    private IFilterBuilder Builder { get; }

    public PatternFilterHeuristicBuilder(IFilterBuilder filterBuilder)
    {
        Builder = filterBuilder;
    }

    public ISiteswapFilter Build(IEnumerable<int> pattern)
    {
        var filter = GenerateAtLeastNumberFilter(pattern);

        return filter;
    }

    private ISiteswapFilter GenerateAtLeastNumberFilter(IEnumerable<int> pattern)
    {
        var result = new List<ISiteswapFilter>();
        foreach (
            var (key, count) in pattern
                .GroupBy(x => x)
                .Where(x => x.Key >= 0)
                .Select(x => (x.Key, x.Count()))
        )
        {
            result.Add(new AtLeastXXXTimesFilter([key], count));
        }

        return new AndFilter(result.ToArray());
    }
}
