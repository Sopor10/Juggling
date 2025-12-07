using System.Collections.Immutable;
using Siteswaps.Generator.Core.Generator.Filter.Combinatorics;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Core.Generator.Filter;

public record FilterBuilder(SiteswapGeneratorInput Input) : IFilterBuilder
{
    private ImmutableList<ISiteswapFilter> Filter { get; init; } =
        ImmutableList<ISiteswapFilter>.Empty;

    public IFilterBuilder MinimumOccurence(IEnumerable<int> number, int amount) =>
        this with
        {
            Filter = Filter.Add(new AtLeastXXXTimesFilter(number, amount)),
        };

    public IFilterBuilder MaximumOccurence(IEnumerable<int> number, int amount) =>
        this with
        {
            Filter = Filter.Add(new AtMostXXXTimesFilter(number, amount)),
        };

    public IFilterBuilder ExactOccurence(IEnumerable<int> number, int amount) =>
        this with
        {
            Filter = Filter.Add(new ExactlyXXXTimesFilter(number, amount)),
        };

    public IFilterBuilder No() => this with { Filter = Filter.Add(new NoFilter()) };

    public IFilterBuilder Not(ISiteswapFilter filter) =>
        this with
        {
            Filter = Filter.Add(new Combinatorics.NotFilter(filter)),
        };

    public IFilterBuilder ExactNumberOfPasses(int numberOfPasses, int numberOfJugglers) =>
        this with
        {
            Filter = Filter.Add(new NumberOfPassesFilter(numberOfPasses, numberOfJugglers, Input)),
        };

    public IFilterBuilder And(params IEnumerable<ISiteswapFilter> filter) =>
        this with
        {
            Filter = Filter.AddRange(filter),
        };

    public IFilterBuilder Or(ISiteswapFilter filter) => Or([filter]);

    public IFilterBuilder Or(params IEnumerable<ISiteswapFilter> filter) =>
        this with
        {
            Filter = [new OrFilter(filter)],
        };

    public IFilterBuilder WithState(State state)
    {
        return this with { Filter = Filter.Add(new StateFilter(Input, state)) };
    }

    public IFilterBuilder FlexiblePattern(
        List<List<int>> pattern,
        int numberOfJuggler,
        bool isGlobalPattern
    )
    {
        return this with
        {
            Filter = Filter.Add(
                new FlexiblePatternFilter(pattern, numberOfJuggler, Input, isGlobalPattern)
            ),
        };
    }

    public IFilterBuilder WithDefault() =>
        this with
        {
            Filter = Filter.Add(new RightAmountOfBallsFilter(Input)),
        };

    public ISiteswapFilter Build() => new AndFilter(Filter);

    public IFilterBuilder Pattern(IEnumerable<int> pattern, int numberOfJuggler)
    {
        pattern = pattern.ToList();
        return this with
        {
            Filter = Filter
                .Add(new PatternFilterHeuristicBuilder(this).Build(pattern))
                .Add(
                    new FlexiblePatternFilter(
                        pattern.Select(x => new List<int>() { x }).ToList(),
                        numberOfJuggler,
                        Input,
                        true
                    )
                ),
        };
    }
}
