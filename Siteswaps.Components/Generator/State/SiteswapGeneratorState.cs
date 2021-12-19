using System;
using System.Collections.Generic;
using Fluxor;
using Siteswaps.Components.Generator.Filter;

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
    
    public List<FilterRendererMap> KnownFilters => new()
    { 
        new(FilterType.Number, typeof(NumberFilter), ()=>new NumberFilterInformation()),
        new(FilterType.Pattern, typeof(PatternFilter), ()=>new PatternFilterInformation()),
    };
    public record FilterRendererMap(FilterType Key, Type ViewType, Func<IFilterInformation> Default);

}