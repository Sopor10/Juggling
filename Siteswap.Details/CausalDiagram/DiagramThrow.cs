namespace Siteswap.Details.CausalDiagram;

/// <summary>A throw node in a completed causal diagram. Its height is derived from its causal arrow.</summary>
public sealed record DiagramThrow(Hand Hand, decimal Time, int? ToLimb = null, int OrbitIndex = -1);
