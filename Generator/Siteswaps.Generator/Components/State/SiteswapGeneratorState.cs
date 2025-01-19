using System.Collections.Immutable;
using Fluxor;
using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Components.State;

[FeatureState]
public record SiteswapGeneratorState(GeneratorState State, ImmutableArray<Siteswap> Siteswaps)
{
    public SiteswapGeneratorState()
        : this(new GeneratorState(), []) { }

    public CancellationTokenSource? CancellationTokenSource { get; init; }
}
