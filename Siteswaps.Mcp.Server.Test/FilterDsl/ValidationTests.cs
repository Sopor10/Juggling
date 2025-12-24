using FluentAssertions;
using Siteswaps.Mcp.Server.Tools.FilterDsl.Ast;
using Siteswaps.Mcp.Server.Tools.FilterDsl.Validation;

namespace Siteswaps.Mcp.Server.Test.FilterDsl;

/// <summary>
/// Tests für die semantische Validierung
/// </summary>
public class ValidationTests
{
    [Test]
    public void Registry_Contains_MinOcc()
    {
        // Act
        var function = FilterFunctionRegistry.GetFunction("minOcc");

        // Assert
        function.Should().NotBeNull();
        function!.Parameters.Should().HaveCount(2);
    }

    [Test]
    public void Registry_Contains_Ground_As_Parameterless_Function()
    {
        // Act
        var function = FilterFunctionRegistry.GetFunction("ground");

        // Assert
        function.Should().NotBeNull();
        function!.Parameters.Should().BeEmpty();
    }

    [Test]
    public void Registry_Is_Case_Insensitive()
    {
        // Act
        var lower = FilterFunctionRegistry.GetFunction("minocc");
        var upper = FilterFunctionRegistry.GetFunction("MINOCC");
        var mixed = FilterFunctionRegistry.GetFunction("MinOcc");

        // Assert
        lower.Should().NotBeNull();
        upper.Should().NotBeNull();
        mixed.Should().NotBeNull();
    }

    [Test]
    public void Registry_Returns_Null_For_Unknown_Function()
    {
        // Act
        var function = FilterFunctionRegistry.GetFunction("unknownFunction");

        // Assert
        function.Should().BeNull();
    }

    [Test]
    public void Registry_Recognizes_Reserved_Keywords()
    {
        // Assert
        FilterFunctionRegistry.IsReservedKeyword("AND").Should().BeTrue();
        FilterFunctionRegistry.IsReservedKeyword("OR").Should().BeTrue();
        FilterFunctionRegistry.IsReservedKeyword("NOT").Should().BeTrue();
        FilterFunctionRegistry.IsReservedKeyword("and").Should().BeTrue();
        FilterFunctionRegistry.IsReservedKeyword("ground").Should().BeFalse();
    }

    [Test]
    public void Registry_Returns_All_Keywords()
    {
        // Act
        var keywords = FilterFunctionRegistry.GetKeywords().ToList();

        // Assert
        keywords.Should().Contain("ground");
        keywords.Should().Contain("excited");
        keywords.Should().Contain("noZeros");
    }

    [Test]
    public void Validate_Known_Identifier_Succeeds()
    {
        // Arrange
        var expr = new FilterExpression.Identifier("ground");

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public void Validate_Known_FunctionCall_With_Correct_Args_Succeeds()
    {
        // Arrange
        var expr = new FilterExpression.FunctionCall(
            "minOcc",
            [new Argument.Number(5), new Argument.Number(2)]
        );

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_And_Expression_With_Valid_Parts_Succeeds()
    {
        // Arrange
        var expr = new FilterExpression.And(
            new FilterExpression.Identifier("ground"),
            new FilterExpression.Identifier("noZeros")
        );

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_Complex_Expression_Succeeds()
    {
        // Arrange
        var expr = new FilterExpression.And(
            new FilterExpression.Or(
                new FilterExpression.FunctionCall(
                    "minOcc",
                    [new Argument.Number(7), new Argument.Number(2)]
                ),
                new FilterExpression.FunctionCall(
                    "exactOcc",
                    [new Argument.Number(9), new Argument.Number(1)]
                )
            ),
            new FilterExpression.Identifier("ground")
        );

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_Pattern_With_Variable_Args_Succeeds()
    {
        // Arrange
        var expr = new FilterExpression.FunctionCall(
            "pattern",
            [new Argument.Number(5), new Argument.Wildcard(), new Argument.Number(1)]
        );

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_MinOcc_With_NumberList_Succeeds()
    {
        // Arrange
        var expr = new FilterExpression.FunctionCall(
            "minOcc",
            [new Argument.NumberList([5, 7]), new Argument.Number(2)]
        );

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_Unknown_Identifier_Fails()
    {
        // Arrange
        var expr = new FilterExpression.Identifier("unknownFunction");

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Type.Should().Be(ValidationErrorType.UnknownFunction);
    }

    [Test]
    public void Validate_Unknown_FunctionCall_Fails()
    {
        // Arrange
        var expr = new FilterExpression.FunctionCall("unknownFunc", [new Argument.Number(5)]);

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors[0].Type.Should().Be(ValidationErrorType.UnknownFunction);
    }

    [Test]
    public void Validate_Too_Few_Arguments_Fails()
    {
        // Arrange
        var expr = new FilterExpression.FunctionCall("minOcc", [new Argument.Number(5)]);

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors[0].Type.Should().Be(ValidationErrorType.InvalidArgumentCount);
    }

    [Test]
    public void Validate_Too_Many_Arguments_Fails()
    {
        // Arrange
        var expr = new FilterExpression.FunctionCall(
            "minOcc",
            [new Argument.Number(5), new Argument.Number(2), new Argument.Number(99)]
        );

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors[0].Type.Should().Be(ValidationErrorType.InvalidArgumentCount);
    }

    [Test]
    public void Validate_Wildcard_Not_Allowed_Fails()
    {
        // Arrange - minOcc erlaubt keine Wildcards
        var expr = new FilterExpression.FunctionCall(
            "minOcc",
            [new Argument.Wildcard(), new Argument.Number(2)]
        );

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors[0].Type.Should().Be(ValidationErrorType.WildcardNotAllowed);
    }

    [Test]
    public void Validate_Reserved_Keyword_As_Identifier_Fails()
    {
        // Arrange
        var expr = new FilterExpression.Identifier("AND");

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors[0].Type.Should().Be(ValidationErrorType.ReservedKeyword);
    }

    [Test]
    public void Validate_Reserved_Keyword_As_FunctionCall_Fails()
    {
        // Arrange
        var expr = new FilterExpression.FunctionCall("OR", [new Argument.Number(5)]);

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors[0].Type.Should().Be(ValidationErrorType.ReservedKeyword);
    }

    [Test]
    public void Validate_Collects_Multiple_Errors()
    {
        // Arrange - Beide Seiten haben Fehler
        var expr = new FilterExpression.And(
            new FilterExpression.Identifier("unknownFunc1"),
            new FilterExpression.Identifier("unknownFunc2")
        );

        // Act
        var result = AstValidator.Validate(expr);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}
