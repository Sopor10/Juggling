using System.Collections;
using System.Collections.Generic;
using Fluxor;

namespace Siteswaps.Components.Generator.State;

[FeatureState]
public record SiteswapGeneratorState
{
    public IReadOnlyCollection<Siteswap> Siteswaps { get; init; }
    public bool IsGenerating => State.IsGenerating;

    public SiteswapGeneratorState()
    {
        State = new GeneratorState();
        Siteswaps = new List<Siteswap>();
    }

    public SiteswapGeneratorState(GeneratorState state, IReadOnlyCollection<Siteswap> siteswaps)
    {
        Siteswaps = siteswaps;
        State = state;
    }

    public GeneratorState State { get; init; }
    
    public KnownFilterTypes KnownFilters => new();

}