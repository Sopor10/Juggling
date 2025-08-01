using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Siteswap.Details.StateDiagram;

[DebuggerDisplay("{PrettyPrint()}")]
public record Transition(Siteswap From, Siteswap To, Throw[] Throws)
{
    public string PrettyPrint()
    {
        return $"{From} -> {To} : {Throws.Aggregate("", (s, i) => s + Siteswap.Transform(i.Value))}";
    }
}
