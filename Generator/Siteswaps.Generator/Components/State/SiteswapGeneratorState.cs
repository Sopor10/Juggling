using Fluxor;
using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Components.State;

using Siteswap = Generator.Siteswap;

[FeatureState]
public record SiteswapGeneratorState(GeneratorState State, IReadOnlyCollection<Siteswap> Siteswaps)
{
    public bool IsGenerating => State.IsGenerating;

    public SiteswapGeneratorState() : this(new GeneratorState(), new List<Siteswap>())
    {
    }

    public KnownFilterTypes KnownFilters => new();

    public IFilterInformation NewFilter { get; init; } = new NumberFilterInformation();

}
