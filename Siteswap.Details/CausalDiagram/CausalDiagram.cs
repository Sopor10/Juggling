using System.Collections.Immutable;

namespace Siteswap.Details.CausalDiagram;

public record CausalDiagram(ImmutableList<Throw> Throws, ImmutableList<Transition> Transitions)
{
    public decimal MaxTime => Throws.Max(x => x.Time);
}
