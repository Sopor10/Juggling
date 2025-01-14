using System.Collections.Immutable;
using Siteswaps.Generator.Generator.Filter;

namespace Siteswaps.Generator.Generator;

public record SiteswapGeneratorFactory
{
    private ImmutableList<Func<IFilterBuilder, IFilterBuilder>> Config { get; init; } =
        ImmutableList<Func<IFilterBuilder, IFilterBuilder>>.Empty;

    public SiteswapGeneratorFactory ConfigureFilter(
        Func<IFilterBuilder, IFilterBuilder>? builder
    ) => this with { Config = builder is null ? Config : Config.Add(builder) };

    public SiteswapGenerator Create(SiteswapGeneratorInput input) =>
        new(
            Config
                .Aggregate(new FilterBuilder().WithInput(input), (current, func) => func(current))
                .WithDefault()
                .Build(),
            input
        );
}
