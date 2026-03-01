using Siteswaps.Generator.Core.Generator.Filter.Combinatorics;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Core.Generator.Filter;

internal class PatternFilterHeuristicBuilder(IFilterBuilder filterBuilder)
{
    private IFilterBuilder Builder { get; } = filterBuilder;

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
