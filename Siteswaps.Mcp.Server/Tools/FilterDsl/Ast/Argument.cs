using Dunet;

namespace Siteswaps.Mcp.Server.Tools.FilterDsl.Ast;

/// <summary>
/// Repräsentiert ein Argument eines Funktionsaufrufs im AST.
/// </summary>
[Union]
public partial record Argument
{
    /// <summary>
    /// Eine einfache Zahl
    /// </summary>
    public partial record Number(int Value);

    /// <summary>
    /// Ein Wildcard (*)
    /// </summary>
    public partial record Wildcard;

    /// <summary>
    /// Eine Liste von Zahlen [5,7,9]
    /// </summary>
    public partial record NumberList(int[] Values);

    /// <summary>
    /// Ein Identifier für zukünftige Erweiterungen
    /// </summary>
    public partial record Id(string Value);

    /// <summary>
    /// Pass-Indikator (p) - nur für Passing-Pattern
    /// </summary>
    public partial record Pass;

    /// <summary>
    /// Self-Indikator (s) - nur für Passing-Pattern
    /// </summary>
    public partial record Self;
}
