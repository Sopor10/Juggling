using System.Diagnostics;

namespace Siteswap.Details.StateDiagram;

[DebuggerDisplay("{PrettyPrint()}")]
public record Transition(Siteswap From, Siteswap To, Throw[] Throws)
{
    public string PrettyPrint()
    {
        return $"{From} -{Throws.Aggregate("", (s, i) => s + Siteswap.Transform(i.Value))}-> {To}";
    }
}
