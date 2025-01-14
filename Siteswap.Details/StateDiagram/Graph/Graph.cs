using System.Collections.Generic;
using System.Linq;

namespace Siteswap.Details.StateDiagram.Graph;

public class Graph<TNode, TData>
{
    public Graph(HashSet<TNode> nodes, HashSet<Edge<TNode, TData>> edges)
    {
        Nodes = nodes;
        Edges = edges;
    }

    public HashSet<TNode> Nodes { get; }
    public HashSet<Edge<TNode, TData>> Edges { get; }

    public Graph<TNode, TData> Combine(Graph<TNode, TData> other) =>
        new(Nodes.Union(other.Nodes).ToHashSet(), Edges.Union(other.Edges).ToHashSet());
}
