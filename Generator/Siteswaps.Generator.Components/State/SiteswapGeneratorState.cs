using Fluxor;
using Siteswaps.Generator.Api;

namespace Siteswaps.Generator.Components.State;

[FeatureState]
public record SiteswapGeneratorState
{
    public IReadOnlyCollection<ISiteswap> Siteswaps { get; init; }
    public bool IsGenerating => State.IsGenerating;

    public SiteswapGeneratorState()
    {
        State = new GeneratorState();
        Siteswaps = new List<ISiteswap>();
    }

    public SiteswapGeneratorState(GeneratorState state, IReadOnlyCollection<ISiteswap> siteswaps)
    {
        Siteswaps = siteswaps;
        State = state;
    }

    public GeneratorState State { get; init; }
    
    public KnownFilterTypes KnownFilters => new();
    
    public IFilterInformation? NewFilter { get; init; }

}