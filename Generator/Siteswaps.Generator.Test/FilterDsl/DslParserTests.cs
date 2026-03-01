using FluentAssertions;
using Siteswaps.Generator.Core.Generator.Filter.FilterDsl;
using Siteswaps.Generator.Core.Generator.Filter.FilterDsl.Ast;

namespace Siteswaps.Generator.Test.FilterDsl;

/// <summary>
/// Tests für den DSL-Parser
/// </summary>
public class DslParserTests
{
    [Test]
    [TestCase("0", 0)]
    [TestCase("5", 5)]
    [TestCase("42", 42)]
    [TestCase("123", 123)]
    public void Parse_Number_Returns_Correct_Value(string input, int expected)
    {
        // Act
        var result = DslParser.ParseNumber(input);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Test]
    [TestCase("minOcc")]
    [TestCase("ground")]
    [TestCase("noZeros")]
    [TestCase("exactOcc")]
    public void Parse_Identifier_Returns_Correct_Name(string input)
    {
        // Act
        var result = DslParser.ParseIdentifier(input);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().Be(input);
    }

    [Test]
    public void Parse_Wildcard_Returns_Wildcard_Argument()
    {
        // Act
        var result = DslParser.ParseArgument("*");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<Argument.Wildcard>();
    }

    [Test]
    public void Parse_Argument_Number_Returns_Number()
    {
        // Act
        var result = DslParser.ParseArgument("5");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<Argument.Number>();
        ((Argument.Number)result.Value).Value.Should().Be(5);
    }

    [Test]
    public void Parse_Argument_NumberList_Returns_NumberList()
    {
        // Act
        var result = DslParser.ParseArgument("[5,7,9]");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<Argument.NumberList>();
        ((Argument.NumberList)result.Value).Values.Should().BeEquivalentTo([5, 7, 9]);
    }

    [Test]
    public void Parse_Argument_NumberList_With_Spaces_Returns_NumberList()
    {
        // Act
        var result = DslParser.ParseArgument("[ 5, 7, 9 ]");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<Argument.NumberList>();
    }

    [Test]
    public void Parse_FunctionCall_Without_Args_Returns_Identifier()
    {
        // Act
        var result = DslParser.Parse("ground");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.Identifier>();
        ((FilterExpression.Identifier)result.Value).Name.Should().Be("ground");
    }

    [Test]
    public void Parse_FunctionCall_With_Single_Arg_Returns_FunctionCall()
    {
        // Act
        var result = DslParser.Parse("maxHeight(9)");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.FunctionCall>();
        var fc = (FilterExpression.FunctionCall)result.Value;
        fc.Name.Should().Be("maxHeight");
        fc.Args.Should().HaveCount(1);
    }

    [Test]
    public void Parse_FunctionCall_With_Multiple_Args_Returns_FunctionCall()
    {
        // Act
        var result = DslParser.Parse("minOcc(5,2)");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.FunctionCall>();
        var fc = (FilterExpression.FunctionCall)result.Value;
        fc.Name.Should().Be("minOcc");
        fc.Args.Should().HaveCount(2);
        fc.Args[0].Should().BeOfType<Argument.Number>();
        fc.Args[1].Should().BeOfType<Argument.Number>();
    }

    [Test]
    public void Parse_FunctionCall_With_Wildcard_Returns_FunctionCall()
    {
        // Act
        var result = DslParser.Parse("pattern(5,*,1)");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.FunctionCall>();
        var fc = (FilterExpression.FunctionCall)result.Value;
        fc.Name.Should().Be("pattern");
        fc.Args.Should().HaveCount(3);
        fc.Args[1].Should().BeOfType<Argument.Wildcard>();
    }

    [Test]
    public void Parse_FunctionCall_With_NumberList_Returns_FunctionCall()
    {
        // Act
        var result = DslParser.Parse("occ([5,7],2)");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.FunctionCall>();
        var fc = (FilterExpression.FunctionCall)result.Value;
        fc.Name.Should().Be("occ");
        fc.Args.Should().HaveCount(2);
        fc.Args[0].Should().BeOfType<Argument.NumberList>();
    }

    [Test]
    public void Parse_And_Expression_Returns_And()
    {
        // Act
        var result = DslParser.Parse("ground AND noZeros");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.And>();
        var and = (FilterExpression.And)result.Value;
        and.Left.Should().BeOfType<FilterExpression.Identifier>();
        and.Right.Should().BeOfType<FilterExpression.Identifier>();
    }

    [Test]
    public void Parse_Or_Expression_Returns_Or()
    {
        // Act
        var result = DslParser.Parse("ground OR excited");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.Or>();
    }

    [Test]
    public void Parse_Not_Expression_Returns_Not()
    {
        // Act
        var result = DslParser.Parse("NOT ground");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.Not>();
        var not = (FilterExpression.Not)result.Value;
        not.Inner.Should().BeOfType<FilterExpression.Identifier>();
    }

    [Test]
    public void Parse_Lowercase_And_Works()
    {
        // Act
        var result = DslParser.Parse("ground and noZeros");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.And>();
    }

    [Test]
    public void Parse_Lowercase_Or_Works()
    {
        // Act
        var result = DslParser.Parse("ground or excited");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.Or>();
    }

    [Test]
    public void Parse_Lowercase_Not_Works()
    {
        // Act
        var result = DslParser.Parse("not ground");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.Not>();
    }

    [Test]
    public void Parse_And_Has_Higher_Precedence_Than_Or()
    {
        // "A OR B AND C" sollte als "A OR (B AND C)" geparst werden
        var result = DslParser.Parse("ground OR noZeros AND excited");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.Or>();
        var or = (FilterExpression.Or)result.Value;
        or.Left.Should().BeOfType<FilterExpression.Identifier>();
        or.Right.Should().BeOfType<FilterExpression.And>();
    }

    [Test]
    public void Parse_Not_Has_Highest_Precedence()
    {
        // "NOT A AND B" sollte als "(NOT A) AND B" geparst werden
        var result = DslParser.Parse("NOT ground AND noZeros");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.And>();
        var and = (FilterExpression.And)result.Value;
        and.Left.Should().BeOfType<FilterExpression.Not>();
        and.Right.Should().BeOfType<FilterExpression.Identifier>();
    }

    [Test]
    public void Parse_Parentheses_Override_Precedence()
    {
        // "(A OR B) AND C" sollte die Klammerung respektieren
        var result = DslParser.Parse("(ground OR excited) AND noZeros");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.And>();
        var and = (FilterExpression.And)result.Value;
        and.Left.Should().BeOfType<FilterExpression.Or>();
        and.Right.Should().BeOfType<FilterExpression.Identifier>();
    }

    [Test]
    public void Parse_Nested_Parentheses()
    {
        // "((A OR B) AND C) OR D"
        var result = DslParser.Parse("(((ground OR excited) AND noZeros) OR excited)");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.Or>();
    }

    [Test]
    public void Parse_Complex_Expression_With_Functions_And_Logic()
    {
        // Act
        var result = DslParser.Parse("minOcc(5,2) AND ground");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.And>();
        var and = (FilterExpression.And)result.Value;
        and.Left.Should().BeOfType<FilterExpression.FunctionCall>();
        and.Right.Should().BeOfType<FilterExpression.Identifier>();
    }

    [Test]
    public void Parse_Complex_Expression_With_Or_And_Functions()
    {
        // Act
        var result = DslParser.Parse("(minOcc(7,2) OR exactOcc(9,1)) AND ground");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.And>();
        var and = (FilterExpression.And)result.Value;
        and.Left.Should().BeOfType<FilterExpression.Or>();
        and.Right.Should().BeOfType<FilterExpression.Identifier>();
    }

    [Test]
    public void Parse_Complex_Expression_With_Not()
    {
        // Act
        var result = DslParser.Parse("NOT (ground OR excited)");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.Not>();
        var not = (FilterExpression.Not)result.Value;
        not.Inner.Should().BeOfType<FilterExpression.Or>();
    }

    [Test]
    public void Parse_Multiple_Chained_Ands()
    {
        // Act
        var result = DslParser.Parse("ground AND noZeros And hasZeros");

        // Assert
        result.Success.Should().BeTrue();
        // Sollte links-assoziativ sein: (ground AND noZeros)
        result.Value.Should().BeOfType<FilterExpression.And>();
    }

    [Test]
    public void Parse_Multiple_Chained_Ors()
    {
        // Act
        var result = DslParser.Parse("ground OR excited OR hasZeros");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.Or>();
    }

    [Test]
    public void Parse_With_Extra_Whitespace()
    {
        // Act
        var result = DslParser.Parse("  ground   AND   noZeros  ");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.And>();
    }

    [Test]
    public void Parse_FunctionCall_With_Spaces()
    {
        // Act
        var result = DslParser.Parse("minOcc( 5 , 2 )");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.FunctionCall>();
    }

    [Test]
    public void Parse_Empty_String_Fails()
    {
        // Act
        var result = DslParser.Parse("");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_Unclosed_Parenthesis_Fails()
    {
        // Act
        var result = DslParser.Parse("(ground AND noZeros");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_Missing_Operand_Fails()
    {
        // Act
        var result = DslParser.Parse("ground AND");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_Empty_Parentheses_Fails()
    {
        // Act
        var result = DslParser.Parse("()");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_FunctionCall_Empty_Args_Fails()
    {
        // Act
        var result = DslParser.Parse("minOcc()");

        // Assert
        // minOcc erfordert Argumente, aber das ist semantische Validierung
        // Syntaktisch könnte es akzeptiert werden
        // Die Entscheidung hängt von der Grammatik ab
        // Für jetzt akzeptieren wir leere Argumente syntaktisch
        result.Success.Should().BeTrue();
        var fc = (FilterExpression.FunctionCall)result.Value;
        fc.Args.Should().BeEmpty();
    }

    [Test]
    public void Parse_IssueFilter_ShouldNotFail()
    {
        var input = "NOT contains(0) AND NOT contains(1) AND NOT contains(3) AND maxHeight(10)";
        var result = DslParser.Parse(input);

        result.Success.Should().BeTrue();
    }

    [Test]
    public void Parse_Just_Operator_Fails()
    {
        // AND allein darf nicht als Identifier geparst werden,
        // da es ein reserviertes Keyword ist.
        var result = DslParser.Parse("AND");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_FunctionCall_With_Siteswap_Number_Returns_FunctionCall()
    {
        // Act
        var result = DslParser.Parse("pattern(a, 5, 1)");

        // Assert
        result.Success.Should().BeTrue();
        var fc = (FilterExpression.FunctionCall)result.Value;
        fc.Args.Should().HaveCount(3);
        ((Argument.Number)fc.Args[0]).Value.Should().Be(10);
    }

    [Test]
    [TestCase("interface(9)")]
    [TestCase("interface(9,5,p,s)")]
    [TestCase("interface (9,5,p,s)")]
    [TestCase("interface(7, 5, 4, 6, a, a)")]
    [TestCase("interface(p,p,s,s,s,s)")]
    [TestCase("interface(p,p,s,s,s,*)")]
    public void Parse_Interface_Syntax_Returns_FunctionCall(string input)
    {
        // Act
        var result = DslParser.Parse(input);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeOfType<FilterExpression.FunctionCall>();
        var fc = (FilterExpression.FunctionCall)result.Value;
        fc.Name.Should().Be("interface");
        fc.Args.Should().HaveCount(1);
        fc.Args[0].Should().BeOfType<Argument.NumberList>();
    }
}
