namespace Siteswaps.Generator.Api.Filter;

public interface IFilterBuilder
{
    IFilterBuilder AddMinimumOccurenceFilter(int number, int amount);
    IFilterBuilder AddMaximumOccurenceFilter(int number, int amount);
    IFilterBuilder AddExactOccurenceFilter(int number, int amount);
    IFilterBuilder AddNoFilter();
    IFilterBuilder AddExactNumberOfPassesFilter(int numberOfPasses, int numberOfJugglers);
    IFilterBuilder AddRange(IEnumerable<ISiteswapFilter> filter);
    IFilterBuilder Add(ISiteswapFilter filter);
    ISiteswapFilter Build();
    IFilterBuilder AddPatternFilter(IEnumerable<int> pattern, int numberOfJuggler);
}

public interface IFilterBuilderFactory
{
    public IFilterBuilder Create(SiteswapGeneratorInput input);
}

