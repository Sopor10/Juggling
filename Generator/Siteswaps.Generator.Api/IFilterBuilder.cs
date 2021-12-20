using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Api;

public interface IFilterBuilder
{
    IFilterBuilder WithInput(SiteswapGeneratorInput input);
    IFilterBuilder AddMinimumOccurenceFilter(int number, int amount);
    IFilterBuilder AddMaximumOccurenceFilter(int number, int amount);
    IFilterBuilder AddExactOccurenceFilter(int number, int amount);
    IFilterBuilder AddNoFilter();
    IFilterBuilder AddExactNumberOfPassesFilter(int numberOfPasses, int numberOfJugglers);
    IFilterBuilder AddRange(IEnumerable<ISiteswapFilter> filter);
    IFilterBuilder Add(ISiteswapFilter filter);
    ISiteswapFilter Build();
}
