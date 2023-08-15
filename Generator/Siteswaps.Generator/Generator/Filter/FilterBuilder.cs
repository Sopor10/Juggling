using System.Collections.Immutable;
using Siteswaps.Generator.Generator.Filter.Combinatorics;

namespace Siteswaps.Generator.Generator.Filter;

internal record FilterBuilder : IFilterBuilder
{
    private FilterFactory Factory { get; init; } = new( new());
    private ImmutableList<ISiteswapFilter> Filter { get; init; } = ImmutableList<ISiteswapFilter>.Empty;

    public IFilterBuilder WithInput(SiteswapGeneratorInput input) =>
        new FilterBuilder
        {
            Factory = new FilterFactory(input)
        };

    public IFilterBuilder MinimumOccurence(int number, int amount) => this with { Filter = Filter.Add(Factory.MinimumOccurenceFilter(number, amount)) };

    public IFilterBuilder MaximumOccurence(int number, int amount) => this with { Filter = Filter.Add(Factory.MaximumOccurenceFilter(number, amount)) };

    public IFilterBuilder ExactOccurence(int number, int amount) => this with { Filter = Filter.Add(Factory.ExactOccurenceFilter(number, amount)) };

    public IFilterBuilder No() => this with { Filter = Filter.Add(Factory.NoFilter()) };

    public IFilterBuilder ExactNumberOfPasses(int numberOfPasses, int numberOfJugglers) => this with { Filter = Filter.Add(Factory.ExactNumberOfPassesFilter(numberOfPasses, numberOfJugglers)) };

    public IFilterBuilder And(ISiteswapFilter filter) => this with { Filter = Filter.Add(filter) };
    public IFilterBuilder Or(ISiteswapFilter filter) => this with { Filter = new[]{Factory.OrFilter(Filter, filter)}.ToImmutableList() };

    public IFilterBuilder FlexiblePattern(Pattern pattern, int numberOfJuggler, bool isGlobalPattern)
    {
        return this with
        {
            Filter = Filter.Add(Factory.FlexiblePatternFilter(pattern, numberOfJuggler, isGlobalPattern))
        };
    }

    public IFilterBuilder WithDefault() => this with { Filter = Filter.Add(Factory.Standard()) };

    public ISiteswapFilter Build() => new AndFilter(Filter);
    public IFilterBuilder Pattern(IEnumerable<int> pattern, int numberOfJuggler)
    {
        pattern = pattern.ToList();
        return this with
        {
            Filter = Filter
                .Add(Factory.GeneratePatternFilterHeuristics(pattern, numberOfJuggler))
                .Add(Factory.FlexiblePatternFilter(Generator.Pattern.FromThrows(pattern, numberOfJuggler), numberOfJuggler, true))
        };
    }
    
    public IFilterBuilder Interface(IEnumerable<int> @interface, int numberOfJuggler)
    {
        @interface = @interface.ToList();
        return this with
        {
            Filter = Filter
                .Add(Factory.InterfaceFilter(Generator.Pattern.FromThrows(@interface, numberOfJuggler), numberOfJuggler, true))
        };
    }
}
