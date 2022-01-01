using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Domain.Filter;

internal class PatternFilterHeuristicBuilder
{
    private FilterFactory Factory { get; }

    public PatternFilterHeuristicBuilder(FilterFactory filterFactory)
    {
        Factory = filterFactory;
    }

    public ISiteswapFilter Build(IEnumerable<int> pattern, int numberOfJuggler, SiteswapGeneratorInput input)
    {
        var filter = GenerateAtLeastNumberFilter(pattern);


        return filter;
    }

    private ISiteswapFilter GenerateAtLeastNumberFilter(IEnumerable<int> pattern)
    {
        var result = new List<ISiteswapFilter>();
        foreach (var (key, count) in pattern.GroupBy(x => x).Where(x => x.Key >= 0).Select(x => (x.Key, x.Count())))
        {
            result.Add(Factory.MinimumOccurenceFilter(key, count));
        }

        return Factory.Combine(result);
    }
}