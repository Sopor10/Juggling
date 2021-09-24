using System.Collections.Generic;

namespace Siteswaps.Graph
{
    public class Graph<TNode, TData>
    {
        public Graph(HashSet<TNode> nodes, HashSet<Edge<TNode, TData>> edges)
        {
            Nodes = nodes;
            Edges = edges;
        }

        public HashSet<TNode> Nodes { get; }
        public HashSet<Edge<TNode, TData>> Edges { get; }
        
    }
}