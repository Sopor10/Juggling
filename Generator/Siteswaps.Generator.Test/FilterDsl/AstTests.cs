using FluentAssertions;
using Siteswaps.Generator.Core.Generator.Filter.FilterDsl.Ast;

namespace Siteswaps.Generator.Test.FilterDsl;

/// <summary>
/// Tests für die AST-Definition
/// </summary>
public class AstTests
{
    [Test]
    public void FilterExpression_And_Can_Be_Created()
    {
        // Arrange & Act
        var left = new FilterExpression.Identifier("ground");
        var right = new FilterExpression.Identifier("noZeros");
        var and = new FilterExpression.And(left, right);

        // Assert
        and.Left.Should().Be(left);
        and.Right.Should().Be(right);
    }

    [Test]
    public void FilterExpression_Or_Can_Be_Created()
    {
        // Arrange & Act
        var left = new FilterExpression.Identifier("ground");
        var right = new FilterExpression.Identifier("excited");
        var or = new FilterExpression.Or(left, right);

        // Assert
        or.Left.Should().Be(left);
        or.Right.Should().Be(right);
    }

    [Test]
    public void FilterExpression_Not_Can_Be_Created()
    {
        // Arrange & Act
        var inner = new FilterExpression.Identifier("ground");
        var not = new FilterExpression.Not(inner);

        // Assert
        not.Inner.Should().Be(inner);
    }

    [Test]
    public void FilterExpression_FunctionCall_Can_Be_Created()
    {
        // Arrange & Act
        var args = new Argument[] { new Argument.Number(5), new Argument.Number(2) };
        var functionCall = new FilterExpression.FunctionCall("minOcc", args);

        // Assert
        functionCall.Name.Should().Be("minOcc");
        functionCall.Args.Should().HaveCount(2);
    }

    [Test]
    public void FilterExpression_Identifier_Can_Be_Created()
    {
        // Arrange & Act
        var identifier = new FilterExpression.Identifier("ground");

        // Assert
        identifier.Name.Should().Be("ground");
    }

    [Test]
    public void Argument_Number_Can_Be_Created()
    {
        // Arrange & Act
        var number = new Argument.Number(42);

        // Assert
        number.Value.Should().Be(42);
    }

    [Test]
    public void Argument_Wildcard_Can_Be_Created()
    {
        // Arrange & Act
        var wildcard = new Argument.Wildcard();

        // Assert
        wildcard.Should().NotBeNull();
    }

    [Test]
    public void Argument_NumberList_Can_Be_Created()
    {
        // Arrange & Act
        var numberList = new Argument.NumberList([5, 7, 9]);

        // Assert
        numberList.Values.Should().BeEquivalentTo([5, 7, 9]);
    }

    [Test]
    public void Argument_Identifier_Can_Be_Created()
    {
        // Arrange & Act
        var identifier = new Argument.Id("someValue");

        // Assert
        identifier.Value.Should().Be("someValue");
    }

    [Test]
    public void Complex_Ast_Can_Be_Built()
    {
        // Arrange & Act
        // Baut: (minOcc(7,2) OR exactOcc(9,1)) AND ground
        var minOcc = new FilterExpression.FunctionCall(
            "minOcc",
            [new Argument.Number(7), new Argument.Number(2)]
        );
        var exactOcc = new FilterExpression.FunctionCall(
            "exactOcc",
            [new Argument.Number(9), new Argument.Number(1)]
        );
        var orExpr = new FilterExpression.Or(minOcc, exactOcc);
        var ground = new FilterExpression.Identifier("ground");
        var andExpr = new FilterExpression.And(orExpr, ground);

        // Assert
        andExpr.Should().NotBeNull();
        andExpr.Left.Should().BeOfType<FilterExpression.Or>();
        andExpr.Right.Should().BeOfType<FilterExpression.Identifier>();
    }

    [Test]
    public void FilterExpression_Can_Be_Pattern_Matched()
    {
        // Arrange
        FilterExpression expr = new FilterExpression.FunctionCall(
            "minOcc",
            [new Argument.Number(5), new Argument.Number(2)]
        );

        // Act
        var result = expr.Match(
            and => "And",
            or => "Or",
            not => "Not",
            functionCall => $"FunctionCall:{functionCall.Name}",
            identifier => $"Identifier:{identifier.Name}"
        );

        // Assert
        result.Should().Be("FunctionCall:minOcc");
    }

    [Test]
    public void Argument_Can_Be_Pattern_Matched()
    {
        // Arrange
        Argument arg = new Argument.Number(42);

        // Act
        var result = arg.Match(
            number => $"Number:{number.Value}",
            wildcard => "Wildcard",
            numberList => $"NumberList:{numberList.Values.Length}",
            id => $"Id:{id.Value}",
            pass => "pass",
            self => "self"
        );

        // Assert
        result.Should().Be("Number:42");
    }
}
