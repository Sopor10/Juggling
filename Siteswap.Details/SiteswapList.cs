using System.Collections.Immutable;
using Siteswap.Details.StateDiagram;
using Siteswap.Details.StateDiagram.Graph;

namespace Siteswap.Details;

public class SiteswapList(ImmutableHashSet<Siteswap> items)
{
    public SiteswapList(params Siteswap[] s)
        : this(s.ToImmutableHashSet()) { }

    public Graph<Siteswap, Transition> TransitionGraph(int length)
    {
        HashSet<Siteswap> nodes = [];
        HashSet<Edge<Siteswap, Transition>> edges = [];

        for (int i = 1; i <= length; i++)
        {
            Console.WriteLine($"[INFO] Starte Iteration mit Länge i={i}");

            foreach (var from in items)
            {
                Console.WriteLine($"[INFO] Betrachte Startknoten: {from}");

                foreach (var to in items.Except([from]))
                {
                    Console.WriteLine($"[INFO]   Betrachte Zielknoten-Kandidat: {to}");

                    Console.WriteLine($"[ACTION]   Füge Knoten hinzu: from = {from}");
                    nodes.Add(from);

                    Console.WriteLine($"[ACTION]   Füge Knoten hinzu: to = {to}");
                    nodes.Add(to);

                    Console.WriteLine(
                        $"[INFO]   Ermittle mögliche Transitionen von {from} nach {to} für Länge {i}"
                    );
                    foreach (var possibleTransition in from.PossibleTransitions(to, i))
                    {
                        Console.WriteLine(
                            $"[ACTION]     Füge Kante hinzu: {possibleTransition.PrettyPrint()}"
                        );
                        edges.Add(new Edge<Siteswap, Transition>(from, to, possibleTransition));
                    }
                }
            }

            Console.WriteLine($"[INFO] Beende Iteration mit Länge i={i}");
        }

        return new Graph<Siteswap, Transition>(nodes, edges);
    }

    public static SiteswapList FromString(string siteswapsString)
    {
        return new SiteswapList(
            siteswapsString
                .Split(",")
                .Select(x =>
                {
                    if (Siteswap.TryCreate(x, out var s) is false)
                    {
                        throw new ArgumentException($"Invalid siteswap: {x}");
                    }

                    return s;
                })
                .ToArray()
        );
    }
}
