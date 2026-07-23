using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Mcp.Server.Tools.FilterDsl;
using Siteswaps.Mcp.Server.Tools.FilterDsl.Ast;
using Siteswaps.Mcp.Server.Tools.FilterDsl.Evaluation;

namespace Siteswaps.Mcp.Server.Test.FilterDsl;

/// <summary>
/// Tests für den FilterCompiler
/// </summary>
public class FilterCompilerTests
{
    private static SiteswapGeneratorInput CreateInput(
        int period = 3,
        int numberOfObjects = 3,
        int minHeight = 1,
        int maxHeight = 7
    )
    {
        return new SiteswapGeneratorInput(period, numberOfObjects, minHeight, maxHeight);
    }

    #region MinOcc Tests

    [Test]
    public void Compile_MinOcc_Creates_Working_Filter()
    {
        // Arrange
        var input = CreateInput();
        var compiler = new FilterCompiler(input);
        var expr = new FilterExpression.FunctionCall(
            "minOcc",
            [new Argument.Number(5), new Argument.Number(2)]
        );

        // Act
        var filter = compiler.Compile(expr);

        // Assert
        filter.Should().NotBeNull();

        // 551 hat zwei 5er - sollte passen
        var siteswap551 = new PartialSiteswap([5, 5, 1]);
        filter.CanFulfill(siteswap551).Should().BeTrue();

        // 531 hat nur eine 5 - sollte nicht passen
        var siteswap531 = new PartialSiteswap([5, 3, 1]);
        filter.CanFulfill(siteswap531).Should().BeFalse();
    }

    [Test]
    public void Compile_MinOcc_With_NumberList_Creates_Working_Filter()
    {
        // Arrange
        var input = CreateInput();
        var compiler = new FilterCompiler(input);
        var expr = new FilterExpression.FunctionCall(
            "minOcc",
            [new Argument.NumberList([5, 7]), new Argument.Number(2)]
        );

        // Act
        var filter = compiler.Compile(expr);

        // Assert
        filter.Should().NotBeNull();
    }

    #endregion

    #region MaxOcc Tests

    [Test]
    public void Compile_MaxOcc_Creates_Working_Filter()
    {
        // Arrange
        var input = CreateInput();
        var compiler = new FilterCompiler(input);
        var expr = new FilterExpression.FunctionCall(
            "maxOcc",
            [new Argument.Number(5), new Argument.Number(1)]
        );

        // Act
        var filter = compiler.Compile(expr);

        // Assert
        filter.Should().NotBeNull();

        // 531 hat nur eine 5 - sollte passen
        var siteswap531 = new PartialSiteswap([5, 3, 1]);
        filter.CanFulfill(siteswap531).Should().BeTrue();

        // 551 hat zwei 5er - sollte nicht passen
        var siteswap551 = new PartialSiteswap([5, 5, 1]);
        filter.CanFulfill(siteswap551).Should().BeFalse();
    }

    #endregion

    #region ExactOcc Tests

    [Test]
    public void Compile_ExactOcc_Creates_Working_Filter()
    {
        // Arrange
        var input = CreateInput();
        var compiler = new FilterCompiler(input);
        var expr = new FilterExpression.FunctionCall(
            "exactOcc",
            [new Argument.Number(5), new Argument.Number(1)]
        );

        // Act
        var filter = compiler.Compile(expr);

        // Assert
        filter.Should().NotBeNull();
    }

    #endregion

    #region Logic Operator Tests

    [Test]
    public void Compile_And_Creates_Working_Filter()
    {
        // Arrange
        var input = CreateInput();
        var compiler = new FilterCompiler(input);
        var expr = new FilterExpression.And(
            new FilterExpression.FunctionCall(
                "minOcc",
                [new Argument.Number(5), new Argument.Number(1)]
            ),
            new FilterExpression.FunctionCall(
                "minOcc",
                [new Argument.Number(3), new Argument.Number(1)]
            )
        );

        // Act
        var filter = compiler.Compile(expr);

        // Assert
        filter.Should().NotBeNull();

        // 531 hat 5 und 3 - sollte passen
        var siteswap531 = new PartialSiteswap([5, 3, 1]);
        filter.CanFulfill(siteswap531).Should().BeTrue();
    }

    [Test]
    public void Compile_Or_Creates_Working_Filter()
    {
        // Arrange
        var input = CreateInput();
        var compiler = new FilterCompiler(input);
        var expr = new FilterExpression.Or(
            new FilterExpression.FunctionCall(
                "minOcc",
                [new Argument.Number(7), new Argument.Number(1)]
            ),
            new FilterExpression.FunctionCall(
                "minOcc",
                [new Argument.Number(5), new Argument.Number(1)]
            )
        );

        // Act
        var filter = compiler.Compile(expr);

        // Assert
        filter.Should().NotBeNull();
    }

    [Test]
    public void Compile_Not_Creates_Working_Filter()
    {
        // Arrange
        var input = CreateInput();
        var compiler = new FilterCompiler(input);
        var expr = new FilterExpression.Not(
            new FilterExpression.FunctionCall(
                "minOcc",
                [new Argument.Number(5), new Argument.Number(2)]
            )
        );

        // Act
        var filter = compiler.Compile(expr);

        // Assert
        filter.Should().NotBeNull();

        // 531 hat nur eine 5 - NOT (min 2 5er) sollte passen
        var siteswap531 = new PartialSiteswap([5, 3, 1]);
        filter.CanFulfill(siteswap531).Should().BeTrue();
    }

    #endregion

    #region Keyword Tests

    [Test]
    public void Compile_NoZeros_Creates_Working_Filter()
    {
        // Arrange
        var input = CreateInput();
        var compiler = new FilterCompiler(input);
        var expr = new FilterExpression.Identifier("noZeros");

        // Act
        var filter = compiler.Compile(expr);

        // Assert
        filter.Should().NotBeNull();

        // 531 hat keine 0 - sollte passen
        var siteswap531 = new PartialSiteswap([5, 3, 1]);
        filter.CanFulfill(siteswap531).Should().BeTrue();
    }

    [Test]
    public void Compile_HasZeros_Creates_Working_Filter()
    {
        // Arrange
        var input = CreateInput(minHeight: 0);
        var compiler = new FilterCompiler(input);
        var expr = new FilterExpression.Identifier("hasZeros");

        // Act
        var filter = compiler.Compile(expr);

        // Assert
        filter.Should().NotBeNull();
    }

    #endregion

    #region Pattern Tests

    [Test]
    public void Compile_Pattern_With_Wildcard_Creates_Filter()
    {
        // Arrange
        var input = CreateInput();
        var compiler = new FilterCompiler(input, numberOfJugglers: 1);
        var expr = new FilterExpression.FunctionCall(
            "pattern",
            [new Argument.Number(5), new Argument.Wildcard(), new Argument.Number(1)]
        );

        // Act
        var filter = compiler.Compile(expr);

        // Assert
        filter.Should().NotBeNull();
    }

    #endregion

    #region Complex Expression Tests

    [Test]
    public void Compile_Complex_Expression_Creates_Working_Filter()
    {
        // Arrange
        // (minOcc(5,1) OR minOcc(7,1)) AND noZeros
        var input = CreateInput();
        var compiler = new FilterCompiler(input);
        var expr = new FilterExpression.And(
            new FilterExpression.Or(
                new FilterExpression.FunctionCall(
                    "minOcc",
                    [new Argument.Number(5), new Argument.Number(1)]
                ),
                new FilterExpression.FunctionCall(
                    "minOcc",
                    [new Argument.Number(7), new Argument.Number(1)]
                )
            ),
            new FilterExpression.Identifier("noZeros")
        );

        // Act
        var filter = compiler.Compile(expr);

        // Assert
        filter.Should().NotBeNull();
    }

    #endregion
}

/// <summary>
/// Tests für die FilterDslParser API-Facade
/// </summary>
public class FilterDslParserApiTests
{
    private static SiteswapGeneratorInput CreateInput() =>
        new(period: 3, numberOfObjects: 3, minHeight: 1, maxHeight: 7);

    [Test]
    public void CreateFilter_With_Valid_Dsl_Returns_Success()
    {
        // Arrange
        var parser = new FilterDslParser(CreateInput());

        // Act
        var result = parser.CreateFilter("minOcc(5,2) AND noZeros");

        // Assert
        result.Success.Should().BeTrue();
        result.Filter.Should().NotBeNull();
        result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void CreateFilter_With_Empty_String_Returns_Failure()
    {
        // Arrange
        var parser = new FilterDslParser(CreateInput());

        // Act
        var result = parser.CreateFilter("");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void CreateFilter_With_Syntax_Error_Returns_Failure()
    {
        // Arrange
        var parser = new FilterDslParser(CreateInput());

        // Act
        var result = parser.CreateFilter("minOcc(5,2 AND noZeros"); // Fehlende Klammer

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Syntaxfehler");
    }

    [Test]
    public void CreateFilter_With_Unknown_Function_Returns_Failure()
    {
        // Arrange
        var parser = new FilterDslParser(CreateInput());

        // Act
        var result = parser.CreateFilter("unknownFunc(5,2)");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Unbekannte Funktion");
    }

    [Test]
    public void CreateFilter_With_Wrong_Arg_Count_Returns_Failure()
    {
        // Arrange
        var parser = new FilterDslParser(CreateInput());

        // Act
        var result = parser.CreateFilter("minOcc(5)"); // Braucht 2 Argumente

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Argument");
    }

    [Test]
    public void CreateFilter_With_Complex_Valid_Dsl_Returns_Success()
    {
        // Arrange
        var parser = new FilterDslParser(CreateInput());

        // Act
        var result = parser.CreateFilter("(minOcc(5,1) OR maxOcc(7,2)) AND noZeros");

        // Assert
        result.Success.Should().BeTrue($"Fehler: {result.ErrorMessage}");
        result.Filter.Should().NotBeNull();
    }

    [Test]
    public void CreateFilter_Filter_Actually_Works()
    {
        // Arrange
        var parser = new FilterDslParser(CreateInput());
        var result = parser.CreateFilter("minOcc(5,2)");

        // Act & Assert
        result.Success.Should().BeTrue();

        var filter = result.Filter!;

        // 551 hat zwei 5er - sollte passen
        filter.CanFulfill(new PartialSiteswap([5, 5, 1])).Should().BeTrue();

        // 531 hat nur eine 5 - sollte nicht passen
        filter.CanFulfill(new PartialSiteswap([5, 3, 1])).Should().BeFalse();
    }
}
