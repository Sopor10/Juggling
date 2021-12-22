using System.Collections.Immutable;
using Siteswaps.Generator.Api;
using Siteswaps.Generator.Api.Filter;

namespace Siteswaps.Generator.Filter;

public record FilterBuilder : IFilterBuilder
{
    private FilterFactory Factory { get; init; } = new( new());
    private ImmutableList<ISiteswapFilter> Filter { get; init; } = ImmutableList<ISiteswapFilter>.Empty;

    public IFilterBuilder WithInput(SiteswapGeneratorInput input) =>
        new FilterBuilder
        {
            Factory = new FilterFactory(input)
        };

    public IFilterBuilder AddMinimumOccurenceFilter(int number, int amount) => this with { Filter = Filter.Add(Factory.MinimumOccurenceFilter(number, amount)) };

    public IFilterBuilder AddMaximumOccurenceFilter(int number, int amount) => this with { Filter = Filter.Add(Factory.MaximumOccurenceFilter(number, amount)) };

    public IFilterBuilder AddExactOccurenceFilter(int number, int amount) => this with { Filter = Filter.Add(Factory.ExactOccurenceFilter(number, amount)) };

    public IFilterBuilder AddNoFilter() => this with { Filter = Filter.Add(Factory.NoFilter()) };

    public IFilterBuilder AddExactNumberOfPassesFilter(int numberOfPasses, int numberOfJugglers) => this with { Filter = Filter.Add(Factory.ExactNumberOfPassesFilter(numberOfPasses, numberOfJugglers)) };

    public IFilterBuilder AddRange(IEnumerable<ISiteswapFilter> filter) => this with { Filter = Filter.AddRange(filter) };

    public IFilterBuilder Add(ISiteswapFilter filter) => this with { Filter = Filter.Add(filter) };

    public ISiteswapFilter Build() => new FilterList(Filter);
    public IFilterBuilder AddPatternFilter(IEnumerable<int> pattern, int numberOfJuggler)
    {
        pattern = pattern.ToList();
        return this with
        {
            Filter = Filter
                .Add(Factory.PatternFilter(pattern, numberOfJuggler))
                .Add(Factory.GeneratePatternFilterHeuristics(pattern, numberOfJuggler))
        };
    }
}