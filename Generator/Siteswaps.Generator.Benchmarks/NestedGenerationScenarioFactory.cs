using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Generator.Core.Generator.Filter.Combinatorics;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

namespace Siteswaps.Generator.Benchmarks;

internal static class NestedGenerationScenarioFactory
{
    public static IReadOnlyList<GenerationScenario> AllScenarios { get; } =
    [
        GenerationScenario.NestedAndNumberPattern,
        GenerationScenario.NestedOrNotState,
        GenerationScenario.NestedStateAndPattern,
        GenerationScenario.NestedDeepMixed,
        GenerationScenario.NestedNumberPassesPersonalized,
    ];

    public static SiteswapGenerator Create(GenerationScenario scenario)
    {
        var input = new SiteswapGeneratorInput(10, 6, 2, 10)
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(60), 1_000),
        };
        ISiteswapFilter filter = scenario switch
        {
            GenerationScenario.NestedAndNumberPattern => new AndFilter(
                DefaultFilter(input),
                new AndFilter(AtMost(input, [5], 6), AtLeast(input, [3, 5, 7], 1)),
                new NotFilter(AtMost(input, [5], 0)),
                CreatePattern(input)
            ),
            GenerationScenario.NestedOrNotState => new OrFilter(
                new AndFilter(DefaultFilter(input), AtMost(input, [5], 6), CreatePattern(input)),
                new NotFilter(new AndFilter(AtMost(input, [5], 0), ExactPasses(input)))
            ),
            GenerationScenario.NestedStateAndPattern => new AndFilter(
                new AndFilter(
                    new FilterBuilder(input).WithState(State.GroundState(6)).Build(),
                    DefaultFilter(input)
                ),
                new OrFilter(AtMost(input, [5], 6), new NotFilter(AtMost(input, [5], 0))),
                CreatePattern(input)
            ),
            GenerationScenario.NestedDeepMixed => new AndFilter(
                new OrFilter(
                    new AndFilter(
                        DefaultFilter(input),
                        AtLeast(input, [3, 5, 7], 1),
                        CreatePattern(input)
                    ),
                    new NotFilter(new AndFilter(AtMost(input, [5], 0), ExactPasses(input)))
                ),
                new NotFilter(new AndFilter(AtMost(input, [5], 6), Personalized(input)))
            ),
            GenerationScenario.NestedNumberPassesPersonalized => new AndFilter(
                new OrFilter(
                    new AndFilter(Personalized(input), AtLeast(input, [3, 5, 7], 1)),
                    new NotFilter(new AndFilter(ExactPasses(input), AtMost(input, [5], 0)))
                ),
                new AndFilter(DefaultFilter(input), CreatePattern(input), AtMost(input, [5], 6))
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null),
        };

        return new SiteswapGenerator(filter, input);
    }

    public static void ValidateResultCount(GenerationScenario scenario, int resultCount)
    {
        if (!AllScenarios.Contains(scenario))
            throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
        if (resultCount < 1_000)
        {
            throw new InvalidOperationException(
                $"Benchmark scenario {scenario} must generate at least 1,000 Siteswaps; got {resultCount}."
            );
        }
    }

    private static ISiteswapFilter DefaultFilter(SiteswapGeneratorInput input) =>
        new FilterBuilder(input).WithDefault().Build();

    private static ISiteswapFilter AtMost(
        SiteswapGeneratorInput input,
        IEnumerable<int> numbers,
        int amount
    ) => new FilterBuilder(input).MaximumOccurence(numbers, amount).Build();

    private static ISiteswapFilter AtLeast(
        SiteswapGeneratorInput input,
        IEnumerable<int> numbers,
        int amount
    ) => new FilterBuilder(input).MinimumOccurence(numbers, amount).Build();

    private static ISiteswapFilter ExactPasses(SiteswapGeneratorInput input) =>
        new FilterBuilder(input).ExactNumberOfPasses(0, 2).Build();

    private static PersonalizedNumberFilter Personalized(SiteswapGeneratorInput input) =>
        new PersonalizedNumberFilter(
            2,
            input.MinHeight,
            input.MaxHeight,
            [6],
            1,
            PersonalizedNumberFilter.Type.AtLeast,
            0
        );

    private static RotationAwareFlexiblePatternFilter CreatePattern(SiteswapGeneratorInput input) =>
        new RotationAwareFlexiblePatternFilter(
            Enumerable.Repeat(new List<int> { -1 }, 5).ToList(),
            2,
            input,
            0
        );
    }
}
