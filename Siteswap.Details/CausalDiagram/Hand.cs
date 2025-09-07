using System.Diagnostics;

namespace Siteswap.Details.CausalDiagram;

[DebuggerDisplay("{Person.Name}_{Name}")]
public record Hand(string Name, Person Person);
