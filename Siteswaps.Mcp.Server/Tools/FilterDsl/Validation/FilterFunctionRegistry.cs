namespace Siteswaps.Mcp.Server.Tools.FilterDsl.Validation;

/// <summary>
/// Registry für alle bekannten Filter-Funktionen.
/// Dient zur semantischen Validierung von Funktionsaufrufen.
/// </summary>
public static class FilterFunctionRegistry
{
    private static readonly Dictionary<string, FunctionDefinition> Functions = new(
        StringComparer.OrdinalIgnoreCase
    );

    static FilterFunctionRegistry()
    {
        RegisterAllFunctions();
    }

    private static void RegisterAllFunctions()
    {
        // Occurrence-Funktionen
        Register(
            new FunctionDefinition(
                "minOcc",
                [
                    new ParameterDefinition("throw", ParameterType.NumberOrList),
                    new ParameterDefinition("count", ParameterType.Number),
                ]
            )
        );

        Register(
            new FunctionDefinition(
                "maxOcc",
                [
                    new ParameterDefinition("throw", ParameterType.NumberOrList),
                    new ParameterDefinition("count", ParameterType.Number),
                ]
            )
        );

        Register(
            new FunctionDefinition(
                "exactOcc",
                [
                    new ParameterDefinition("throw", ParameterType.NumberOrList),
                    new ParameterDefinition("count", ParameterType.Number),
                ]
            )
        );

        Register(
            new FunctionDefinition(
                "occ",
                [
                    new ParameterDefinition("throw", ParameterType.NumberOrList),
                    new ParameterDefinition("min", ParameterType.Number),
                    new ParameterDefinition("max", ParameterType.Number),
                ]
            )
        );

        // Pattern-Funktionen
        Register(new FunctionDefinition("pattern", [], AllowsVariableArgs: true));
        Register(new FunctionDefinition("startsWith", [], AllowsVariableArgs: true));
        Register(new FunctionDefinition("endsWith", [], AllowsVariableArgs: true));
        Register(new FunctionDefinition("contains", [], AllowsVariableArgs: true));

        // Höhen-Funktionen
        Register(
            new FunctionDefinition(
                "height",
                [
                    new ParameterDefinition("min", ParameterType.Number),
                    new ParameterDefinition("max", ParameterType.Number),
                ]
            )
        );

        Register(
            new FunctionDefinition(
                "maxHeight",
                [new ParameterDefinition("max", ParameterType.Number)]
            )
        );

        Register(
            new FunctionDefinition(
                "minHeight",
                [new ParameterDefinition("min", ParameterType.Number)]
            )
        );

        // Orbit/Pass-Funktionen
        Register(
            new FunctionDefinition(
                "orbits",
                [new ParameterDefinition("count", ParameterType.Number)]
            )
        );

        Register(
            new FunctionDefinition(
                "passes",
                [new ParameterDefinition("count", ParameterType.Number)]
            )
        );

        Register(
            new FunctionDefinition(
                "passRatio",
                [
                    new ParameterDefinition("min", ParameterType.Number),
                    new ParameterDefinition("max", ParameterType.Number),
                ]
            )
        );

        // State-Funktion
        Register(new FunctionDefinition("state", [], AllowsVariableArgs: true));

        // Interface-Funktion
        Register(
            new FunctionDefinition(
                "interface",
                [new ParameterDefinition("symbols", ParameterType.NumberOrList)]
            )
        );

        // Parameterlose Keywords (registriert als Funktionen ohne Parameter)
        Register(new FunctionDefinition("ground", []));
        Register(new FunctionDefinition("excited", []));
        Register(new FunctionDefinition("noZeros", []));
        Register(new FunctionDefinition("hasZeros", []));
    }

    private static void Register(FunctionDefinition definition)
    {
        Functions[definition.Name] = definition;
    }

    /// <summary>
    /// Gibt die Funktionsdefinition für einen Namen zurück, falls vorhanden
    /// </summary>
    public static FunctionDefinition? GetFunction(string name)
    {
        return Functions.TryGetValue(name, out var definition) ? definition : null;
    }

    /// <summary>
    /// Prüft, ob eine Funktion mit dem Namen existiert
    /// </summary>
    public static bool FunctionExists(string name)
    {
        return Functions.ContainsKey(name);
    }

    /// <summary>
    /// Gibt alle registrierten Funktionen zurück
    /// </summary>
    public static IEnumerable<FunctionDefinition> GetAllFunctions()
    {
        return Functions.Values;
    }

    /// <summary>
    /// Gibt alle parameterlosen Funktionen (Keywords) zurück
    /// </summary>
    public static IEnumerable<string> GetKeywords()
    {
        return Functions
            .Values.Where(f => f.Parameters.Length == 0 && !f.AllowsVariableArgs)
            .Select(f => f.Name);
    }

    /// <summary>
    /// Prüft, ob ein Name ein reserviertes Keyword ist
    /// </summary>
    public static bool IsReservedKeyword(string name)
    {
        var upper = name.ToUpperInvariant();
        return upper is "AND" or "OR" or "NOT";
    }
}
