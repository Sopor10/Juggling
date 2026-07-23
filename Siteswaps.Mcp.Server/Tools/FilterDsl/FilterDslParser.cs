using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Mcp.Server.Tools.FilterDsl.Ast;
using Siteswaps.Mcp.Server.Tools.FilterDsl.Evaluation;
using Siteswaps.Mcp.Server.Tools.FilterDsl.Validation;

namespace Siteswaps.Mcp.Server.Tools.FilterDsl;

/// <summary>
/// Haupteinstiegspunkt für die Filter-DSL.
/// Kombiniert Parsing, Validierung und Kompilierung.
/// </summary>
public class FilterDslParser
{
    private readonly SiteswapGeneratorInput _input;
    private readonly int? _numberOfJugglers;

    public FilterDslParser(SiteswapGeneratorInput input, int? numberOfJugglers = null)
    {
        _input = input;
        _numberOfJugglers = numberOfJugglers;
    }

    /// <summary>
    /// Parst einen DSL-String in einen AST
    /// </summary>
    public FilterParseResult Parse(string dsl)
    {
        if (string.IsNullOrWhiteSpace(dsl))
        {
            return FilterParseResult.Failure("DSL-String darf nicht leer sein.");
        }

        var parseResult = DslParser.Parse(dsl);

        if (!parseResult.Success)
        {
            var errorMessage = $"Syntaxfehler: {parseResult.Error}";
            return FilterParseResult.Failure(errorMessage);
        }

        return FilterParseResult.Successful(parseResult.Value);
    }

    /// <summary>
    /// Validiert einen geparsten AST
    /// </summary>
    public ValidationResult Validate(FilterExpression expression)
    {
        return AstValidator.Validate(expression);
    }

    /// <summary>
    /// Kompiliert einen validierten AST in einen Filter
    /// </summary>
    public ISiteswapFilter Compile(FilterExpression expression)
    {
        var compiler = new FilterCompiler(_input, _numberOfJugglers);
        return compiler.Compile(expression);
    }

    /// <summary>
    /// Parst, validiert und kompiliert einen DSL-String in einem Schritt.
    /// Dies ist die Hauptmethode für die Verwendung im MCP-Server.
    /// </summary>
    public FilterCreateResult CreateFilter(string dsl)
    {
        // 1. Parsen
        var parseResult = Parse(dsl);
        if (!parseResult.Success)
        {
            return FilterCreateResult.Failure(parseResult.ErrorMessage!);
        }

        // 2. Validieren
        var validationResult = Validate(parseResult.Expression!);
        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(e => e.Message);
            return FilterCreateResult.Failure(string.Join("; ", errorMessages));
        }

        // 3. Kompilieren
        try
        {
            var filter = Compile(parseResult.Expression!);
            return FilterCreateResult.Successful(filter);
        }
        catch (Exception ex)
        {
            return FilterCreateResult.Failure($"Kompilierungsfehler: {ex.Message}");
        }
    }
}

/// <summary>
/// Ergebnis des Parsens
/// </summary>
public record FilterParseResult
{
    public bool Success { get; init; }
    public FilterExpression? Expression { get; init; }
    public string? ErrorMessage { get; init; }

    public static FilterParseResult Successful(FilterExpression expression) =>
        new() { Success = true, Expression = expression };

    public static FilterParseResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Ergebnis der Filter-Erstellung
/// </summary>
public record FilterCreateResult
{
    public bool Success { get; init; }
    public ISiteswapFilter? Filter { get; init; }
    public string? ErrorMessage { get; init; }

    public static FilterCreateResult Successful(ISiteswapFilter filter) =>
        new() { Success = true, Filter = filter };

    public static FilterCreateResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
