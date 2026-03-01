namespace Siteswap.Details.StateDiagram.Graph;

public class Graph<TNode, TData>(HashSet<TNode> nodes, HashSet<Edge<TNode, TData>> edges)
{
    public HashSet<TNode> Nodes { get; } = nodes;
    public HashSet<Edge<TNode, TData>> Edges { get; } = edges;

    public Graph<TNode, TData> Combine(Graph<TNode, TData> other) =>
        new(Nodes.Union(other.Nodes).ToHashSet(), Edges.Union(other.Edges).ToHashSet());
}
