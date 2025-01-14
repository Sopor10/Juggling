namespace Siteswap.Details.StateDiagram.Graph;

public record Edge<TNode, TData>(TNode N1, TNode N2, TData Data)
{
    public override string ToString() => $"{N1} - {N2} : {Data}";
}
