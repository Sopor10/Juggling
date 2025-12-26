using Siteswaps.Generator.Core.Generator.Filter.FilterDsl.Ast;

namespace Siteswaps.Generator.Core.Generator.Filter.FilterDsl.Validation;

/// <summary>
/// Validiert den AST semantisch
/// </summary>
public static class AstValidator
{
    /// <summary>
    /// Validiert einen Filter-Ausdruck
    /// </summary>
    public static ValidationResult Validate(FilterExpression expression)
    {
        return expression.Match(
            and => Validate(and.Left).Merge(Validate(and.Right)),
            or => Validate(or.Left).Merge(Validate(or.Right)),
            not => Validate(not.Inner),
            functionCall => ValidateFunctionCall(functionCall),
            identifier => ValidateIdentifier(identifier)
        );
    }

    private static ValidationResult ValidateIdentifier(FilterExpression.Identifier identifier)
    {
        // Prüfe auf reservierte Keywords
        if (FilterFunctionRegistry.IsReservedKeyword(identifier.Name))
        {
            return ValidationResult.Failure(
                new ValidationError(
                    ValidationErrorType.ReservedKeyword,
                    $"'{identifier.Name}' ist ein reserviertes Keyword und kann nicht als Funktionsname verwendet werden.",
                    identifier.Name
                )
            );
        }

        // Prüfe, ob die Funktion bekannt ist (parameterlose Funktion)
        var function = FilterFunctionRegistry.GetFunction(identifier.Name);
        if (function == null)
        {
            return ValidationResult.Failure(
                new ValidationError(
                    ValidationErrorType.UnknownFunction,
                    $"Unbekannte Funktion: '{identifier.Name}'",
                    identifier.Name
                )
            );
        }

        return ValidationResult.Success();
    }

    private static ValidationResult ValidateFunctionCall(FilterExpression.FunctionCall functionCall)
    {
        // Prüfe auf reservierte Keywords
        if (FilterFunctionRegistry.IsReservedKeyword(functionCall.Name))
        {
            return ValidationResult.Failure(
                new ValidationError(
                    ValidationErrorType.ReservedKeyword,
                    $"'{functionCall.Name}' ist ein reserviertes Keyword und kann nicht als Funktionsname verwendet werden.",
                    functionCall.Name
                )
            );
        }

        // Prüfe, ob die Funktion bekannt ist
        var function = FilterFunctionRegistry.GetFunction(functionCall.Name);
        if (function == null)
        {
            return ValidationResult.Failure(
                new ValidationError(
                    ValidationErrorType.UnknownFunction,
                    $"Unbekannte Funktion: '{functionCall.Name}'",
                    functionCall.Name
                )
            );
        }

        // Prüfe Argumentanzahl
        if (!function.AllowsVariableArgs)
        {
            var requiredCount = function.Parameters.Count(p => !p.IsOptional);
            var maxCount = function.Parameters.Length;

            if (functionCall.Args.Length < requiredCount)
            {
                return ValidationResult.Failure(
                    new ValidationError(
                        ValidationErrorType.InvalidArgumentCount,
                        $"Funktion '{functionCall.Name}' erwartet mindestens {requiredCount} Argument(e), aber {functionCall.Args.Length} wurden übergeben.",
                        functionCall.Name
                    )
                );
            }

            if (functionCall.Args.Length > maxCount)
            {
                return ValidationResult.Failure(
                    new ValidationError(
                        ValidationErrorType.InvalidArgumentCount,
                        $"Funktion '{functionCall.Name}' erwartet maximal {maxCount} Argument(e), aber {functionCall.Args.Length} wurden übergeben.",
                        functionCall.Name
                    )
                );
            }
        }

        // Prüfe Argumenttypen
        var errors = new List<ValidationError>();
        for (var i = 0; i < functionCall.Args.Length && i < function.Parameters.Length; i++)
        {
            var arg = functionCall.Args[i];
            var param = function.Parameters[i];
            var argError = ValidateArgumentType(arg, param, functionCall.Name);
            if (argError != null)
            {
                errors.Add(argError);
            }
        }

        return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    private static ValidationError? ValidateArgumentType(
        Argument arg,
        ParameterDefinition param,
        string functionName
    )
    {
        return arg.Match<ValidationError?>(
            number =>
                param.Type switch
                {
                    ParameterType.Number => null,
                    ParameterType.NumberOrList => null,
                    ParameterType.NumberOrWildcard => null,
                    ParameterType.Any => null,
                    _ => null,
                },
            wildcard =>
                param.Type switch
                {
                    ParameterType.NumberOrWildcard => null,
                    ParameterType.Any => null,
                    _ => new ValidationError(
                        ValidationErrorType.WildcardNotAllowed,
                        $"Wildcard (*) ist für Parameter '{param.Name}' in Funktion '{functionName}' nicht erlaubt.",
                        functionName
                    ),
                },
            numberList =>
                param.Type switch
                {
                    ParameterType.NumberOrList => null,
                    ParameterType.Any => null,
                    _ => new ValidationError(
                        ValidationErrorType.InvalidArgumentType,
                        $"Nummernliste ist für Parameter '{param.Name}' in Funktion '{functionName}' nicht erlaubt.",
                        functionName
                    ),
                },
            id => null, // Identifier sind für zukünftige Erweiterungen vorgesehen
            pass =>
                param.Type switch
                {
                    ParameterType.NumberOrWildcard => null,
                    ParameterType.Any => null,
                    _ => new ValidationError(
                        ValidationErrorType.InvalidArgumentType,
                        $"Pass (p) ist für Parameter '{param.Name}' in Funktion '{functionName}' nicht erlaubt.",
                        functionName
                    ),
                },
            self =>
                param.Type switch
                {
                    ParameterType.NumberOrWildcard => null,
                    ParameterType.Any => null,
                    _ => new ValidationError(
                        ValidationErrorType.InvalidArgumentType,
                        $"Self (s) ist für Parameter '{param.Name}' in Funktion '{functionName}' nicht erlaubt.",
                        functionName
                    ),
                }
        );
    }
}
