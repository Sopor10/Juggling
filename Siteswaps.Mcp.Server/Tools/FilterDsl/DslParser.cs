using Pidgin;
using Pidgin.Expression;
using Siteswaps.Mcp.Server.Tools.FilterDsl.Ast;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Siteswaps.Mcp.Server.Tools.FilterDsl;

/// <summary>
/// Parser für die Filter-DSL basierend auf Pidgin.
/// </summary>
public static class DslParser
{
    #region Primitive Parsers

    /// <summary>
    /// Parser für Whitespace (wird übersprungen)
    /// </summary>
    private static readonly Parser<char, Unit> Whitespace = SkipWhitespaces;

    /// <summary>
    /// Parser für eine einzelne Ziffer
    /// </summary>
    private static readonly Parser<char, char> Digit = Token(char.IsDigit);

    /// <summary>
    /// Parser für Integer-Zahlen
    /// </summary>
    private static readonly Parser<char, int> NumberParser = Digit
        .AtLeastOnceString()
        .Select(int.Parse)
        .Labelled("number");

    /// <summary>
    /// Parser für Identifier (Funktionsnamen, Keywords)
    /// </summary>
    private static readonly Parser<char, string> IdentifierParser = Token(char.IsLetter)
        .Then(
            Token(c => char.IsLetterOrDigit(c) || c == '_').ManyString(),
            (first, rest) => first + rest
        )
        .Labelled("identifier");

    /// <summary>
    /// Parser für Wildcard (*)
    /// </summary>
    private static readonly Parser<char, Argument> WildcardParser = Char('*')
        .ThenReturn<Argument>(new Argument.Wildcard());

    /// <summary>
    /// Parser für Pass (p)
    /// </summary>
    private static readonly Parser<char, Argument> PassParser = Char('p')
        .ThenReturn<Argument>(new Argument.Pass());

    /// <summary>
    /// Parser für Self (s)
    /// </summary>
    private static readonly Parser<char, Argument> SelfParser = Char('s')
        .ThenReturn<Argument>(new Argument.Self());

    #endregion

    #region Argument Parsers

    /// <summary>
    /// Parser für Number-Argument
    /// </summary>
    private static readonly Parser<char, Argument> NumberArgParser = NumberParser.Select<Argument>(
        n => new Argument.Number(n)
    );

    /// <summary>
    /// Parser für NumberList-Argument [5,7,9]
    /// </summary>
    private static readonly Parser<char, Argument> NumberListParser = Char('[')
        .Then(Whitespace)
        .Then(NumberParser.Between(Whitespace).Separated(Char(',')))
        .Before(Whitespace)
        .Before(Char(']'))
        .Select<Argument>(nums => new Argument.NumberList(nums.ToArray()));

    /// <summary>
    /// Parser für alle Argument-Typen
    /// </summary>
    private static readonly Parser<char, Argument> ArgumentParser = OneOf(
            WildcardParser,
            PassParser,
            SelfParser,
            NumberListParser,
            NumberArgParser
        )
        .Between(Whitespace);

    /// <summary>
    /// Parser für Argument-Liste (arg1, arg2, ...)
    /// </summary>
    private static readonly Parser<char, Argument[]> ArgListParser = ArgumentParser
        .Separated(Char(','))
        .Select(args => args.ToArray());

    #endregion

    #region Expression Parsers

    /// <summary>
    /// Parser für Funktionsaufrufe oder Identifier
    /// </summary>
    private static readonly Parser<char, FilterExpression> AtomParser = IdentifierParser
        .Before(Whitespace)
        .Then(
            Char('(')
                .Then(Whitespace)
                .Then(ArgListParser)
                .Before(Whitespace)
                .Before(Char(')'))
                .Optional(),
            (name, args) =>
                args.HasValue
                    ? (FilterExpression)new FilterExpression.FunctionCall(name, args.Value)
                    : new FilterExpression.Identifier(name)
        );

    /// <summary>
    /// Parser für AND-Keyword (case-insensitive)
    /// Erfordert Whitespace davor und danach
    /// </summary>
    private static readonly Parser<char, Unit> AndKeyword = Try(
            Whitespace
                .Then(CIString("AND"))
                .Before(
                    Lookahead(
                        OneOf(Whitespace.Then(Return(Unit.Value)), Char('(').ThenReturn(Unit.Value))
                    )
                )
        )
        .IgnoreResult()
        .Labelled("AND");

    /// <summary>
    /// Parser für OR-Keyword (case-insensitive)
    /// </summary>
    private static readonly Parser<char, Unit> OrKeyword = Try(
            Whitespace
                .Then(CIString("OR"))
                .Before(
                    Lookahead(
                        OneOf(Whitespace.Then(Return(Unit.Value)), Char('(').ThenReturn(Unit.Value))
                    )
                )
        )
        .IgnoreResult()
        .Labelled("OR");

    /// <summary>
    /// Parser für NOT-Keyword (case-insensitive)
    /// </summary>
    private static readonly Parser<char, Unit> NotKeyword = Try(
            CIString("NOT")
                .Before(
                    Lookahead(
                        OneOf(Whitespace.Then(Return(Unit.Value)), Char('(').ThenReturn(Unit.Value))
                    )
                )
        )
        .IgnoreResult()
        .Labelled("NOT");

    /// <summary>
    /// Der vollständige Expression-Parser mit korrekter Operator-Präzedenz
    /// </summary>
    private static readonly Parser<char, FilterExpression> ExpressionParserInternal =
        Pidgin.Expression.ExpressionParser.Build<char, FilterExpression>(
            expr =>
                OneOf(
                        expr.Between(Char('(').Before(Whitespace), Whitespace.Then(Char(')'))),
                        AtomParser
                    )
                    .Between(Whitespace),
            new[]
            {
                // Höchste Präzedenz: NOT (Prefix)
                Operator.Prefix(
                    NotKeyword.ThenReturn<Func<FilterExpression, FilterExpression>>(
                        inner => new FilterExpression.Not(inner)
                    )
                ),
                // Mittlere Präzedenz: AND (links-assoziativ)
                Operator.InfixL(
                    AndKeyword.ThenReturn<
                        Func<FilterExpression, FilterExpression, FilterExpression>
                    >((left, right) => new FilterExpression.And(left, right))
                ),
                // Niedrigste Präzedenz: OR (links-assoziativ)
                Operator.InfixL(
                    OrKeyword.ThenReturn<
                        Func<FilterExpression, FilterExpression, FilterExpression>
                    >((left, right) => new FilterExpression.Or(left, right))
                ),
            }
        );

    /// <summary>
    /// Der Haupt-Parser für die gesamte Eingabe
    /// </summary>
    private static readonly Parser<char, FilterExpression> MainParser = Whitespace
        .Then(ExpressionParserInternal)
        .Before(Whitespace)
        .Before(End);

    #endregion

    #region Public API

    /// <summary>
    /// Parst einen DSL-String in einen Filter-AST
    /// </summary>
    public static Result<char, FilterExpression> Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            // Bei leerem Input einen Parser verwenden, der scheitert
            return Fail<FilterExpression>().Parse("");
        }

        return MainParser.Parse(input);
    }

    /// <summary>
    /// Parst eine Zahl (für Tests)
    /// </summary>
    public static Result<char, int> ParseNumber(string input)
    {
        return NumberParser.Before(End).Parse(input);
    }

    /// <summary>
    /// Parst einen Identifier (für Tests)
    /// </summary>
    public static Result<char, string> ParseIdentifier(string input)
    {
        return IdentifierParser.Before(End).Parse(input);
    }

    /// <summary>
    /// Parst ein Argument (für Tests)
    /// </summary>
    public static Result<char, Argument> ParseArgument(string input)
    {
        return ArgumentParser.Before(End).Parse(input);
    }

    #endregion
}
