namespace Siteswaps.Mcp.Server.Tools.FilterDsl.Validation;

/// <summary>
/// Ergebnis einer Validierung
/// </summary>
public record ValidationResult
{
    public bool IsValid { get; init; }
    public List<ValidationError> Errors { get; init; } = [];

    public static ValidationResult Success() => new() { IsValid = true };

    public static ValidationResult Failure(params ValidationError[] errors) =>
        new() { IsValid = false, Errors = [.. errors] };

    public static ValidationResult Failure(IEnumerable<ValidationError> errors) =>
        new() { IsValid = false, Errors = errors.ToList() };

    public ValidationResult Merge(ValidationResult other)
    {
        if (IsValid && other.IsValid)
            return Success();

        return Failure(Errors.Concat(other.Errors));
    }
}

/// <summary>
/// Ein Validierungsfehler
/// </summary>
public record ValidationError(
    ValidationErrorType Type,
    string Message,
    string? FunctionName = null
);

/// <summary>
/// Typen von Validierungsfehlern
/// </summary>
public enum ValidationErrorType
{
    UnknownFunction,
    InvalidArgumentCount,
    InvalidArgumentType,
    ReservedKeyword,
    WildcardNotAllowed,
}
