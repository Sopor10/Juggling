using Fluxor;
using Siteswaps.Generator.Api;

namespace Siteswaps.Generator.Components.State;

[FeatureState]
public record SiteswapGeneratorState(GeneratorState State, IReadOnlyCollection<ISiteswap> Siteswaps)
{
    public bool IsGenerating => State.IsGenerating;

    public SiteswapGeneratorState() : this(new GeneratorState(), new List<ISiteswap>())
    {
    }

    public KnownFilterTypes KnownFilters => new();

    public IFilterInformation NewFilter { get; init; } = new NumberFilterInformation();

}