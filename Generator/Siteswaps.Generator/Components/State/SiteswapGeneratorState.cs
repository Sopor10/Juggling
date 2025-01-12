using Fluxor;
using Siteswaps.Generator.Components.Internal.EasyFilter;
using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Components.State;

[FeatureState]
public record SiteswapGeneratorState(GeneratorState State, IReadOnlyCollection<Siteswap> Siteswaps)
{
    public bool IsGenerating => State.IsGenerating;

    public SiteswapGeneratorState() : this(new GeneratorState(), new List<Siteswap>())
    {
    }

    public IFilterInformation? NewFilter { get; init; }

}
