using System.Diagnostics.CodeAnalysis;

namespace Siteswap.Details.CausalDiagram;

[SuppressMessage("Naming", "CA1716", Justification = "Throw is established domain vocabulary.")]
public record Throw(Hand Hand, int Height, decimal Time);
