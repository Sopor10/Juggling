namespace Siteswap.Details.StateDiagram;

using System.Linq;

public record Transition(Siteswap From, Siteswap To, int[] Throws)
{
    public string PrettyPrint()
    {
        return $"{From} -> {To} : {Throws.Aggregate("", (s, i) => s + Siteswap.Transform(i))}";
    }
}