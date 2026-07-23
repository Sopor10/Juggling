using Dunet;

namespace Siteswaps.Mcp.Server.Tools.FilterDsl.Ast;

/// <summary>
/// Repräsentiert einen Filter-Ausdruck im AST.
/// Der AST ist rein syntaktisch - keine Domänenlogik.
/// </summary>
[Union]
public partial record FilterExpression
{
    /// <summary>
    /// AND-Verknüpfung zweier Ausdrücke
    /// </summary>
    public partial record And(FilterExpression Left, FilterExpression Right);

    /// <summary>
    /// OR-Verknüpfung zweier Ausdrücke
    /// </summary>
    public partial record Or(FilterExpression Left, FilterExpression Right);

    /// <summary>
    /// Negation eines Ausdrucks
    /// </summary>
    public partial record Not(FilterExpression Inner);

    /// <summary>
    /// Funktionsaufruf mit Name und Argumenten
    /// </summary>
    public partial record FunctionCall(string Name, Argument[] Args);

    /// <summary>
    /// Parameterlose Keywords wie 'ground', 'noZeros'
    /// </summary>
    public partial record Identifier(string Name);
}
