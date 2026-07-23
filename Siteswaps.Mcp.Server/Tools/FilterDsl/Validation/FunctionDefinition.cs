namespace Siteswaps.Mcp.Server.Tools.FilterDsl.Validation;

/// <summary>
/// Definiert eine Filter-Funktion mit ihren erlaubten Parametern
/// </summary>
public record FunctionDefinition(
    string Name,
    ParameterDefinition[] Parameters,
    bool AllowsVariableArgs = false
);

/// <summary>
/// Definiert einen Parameter einer Funktion
/// </summary>
public record ParameterDefinition(string Name, ParameterType Type, bool IsOptional = false);

/// <summary>
/// Die möglichen Typen für Parameter
/// </summary>
public enum ParameterType
{
    /// <summary>Eine einzelne Zahl</summary>
    Number,

    /// <summary>Eine Liste von Zahlen oder eine einzelne Zahl</summary>
    NumberOrList,

    /// <summary>Wildcard (*) erlaubt</summary>
    NumberOrWildcard,

    /// <summary>Zahl, Liste oder Wildcard</summary>
    Any,
}
