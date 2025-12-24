using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Mcp.Server.Tools.FilterDsl.Ast;

namespace Siteswaps.Mcp.Server.Tools.FilterDsl.Evaluation;

/// <summary>
/// Kompiliert einen validierten AST in einen ausführbaren Filter
/// </summary>
public class FilterCompiler(SiteswapGeneratorInput input, int? numberOfJugglers = null)
{
    /// <summary>
    /// Kompiliert einen Filter-Ausdruck in einen ISiteswapFilter
    /// </summary>
    public ISiteswapFilter Compile(FilterExpression expression)
    {
        return expression.Match(
            CompileAnd,
            CompileOr,
            CompileNot,
            CompileFunctionCall,
            CompileIdentifier
        );
    }

    private ISiteswapFilter CompileAnd(FilterExpression.And and)
    {
        var leftFilter = Compile(and.Left);
        var rightFilter = Compile(and.Right);
        return new FilterBuilder(input).And(leftFilter, rightFilter).Build();
    }

    private ISiteswapFilter CompileOr(FilterExpression.Or or)
    {
        var leftFilter = Compile(or.Left);
        var rightFilter = Compile(or.Right);
        return new FilterBuilder(input).Or(leftFilter, rightFilter).Build();
    }

    private ISiteswapFilter CompileNot(FilterExpression.Not not)
    {
        var innerFilter = Compile(not.Inner);
        return new FilterBuilder(input).Not(innerFilter).Build();
    }

    private ISiteswapFilter CompileIdentifier(FilterExpression.Identifier identifier)
    {
        // Parameterlose Keywords
        return identifier.Name.ToLowerInvariant() switch
        {
            "ground" => CompileGroundFilter(),
            "excited" => CompileExcitedFilter(),
            "nozeros" => CompileNoZerosFilter(),
            "haszeros" => CompileHasZerosFilter(),
            _ => throw new InvalidOperationException($"Unbekannter Identifier: {identifier.Name}"),
        };
    }

    private ISiteswapFilter CompileFunctionCall(FilterExpression.FunctionCall functionCall)
    {
        return functionCall.Name.ToLowerInvariant() switch
        {
            "minocc" => CompileMinOcc(functionCall.Args),
            "maxocc" => CompileMaxOcc(functionCall.Args),
            "exactocc" => CompileExactOcc(functionCall.Args),
            "occ" => CompileOcc(functionCall.Args),
            "pattern" => CompilePattern(functionCall.Args),
            "startswith" => CompileStartsWith(functionCall.Args),
            "endswith" => CompileEndsWith(functionCall.Args),
            "contains" => CompileContains(functionCall.Args),
            "height" => CompileHeight(functionCall.Args),
            "maxheight" => CompileMaxHeight(functionCall.Args),
            "minheight" => CompileMinHeight(functionCall.Args),
            "orbits" => CompileOrbits(functionCall.Args),
            "passes" => CompilePasses(functionCall.Args),
            "state" => CompileState(functionCall.Args),
            "interface" => CompileInterface(functionCall.Args),
            _ => throw new InvalidOperationException($"Unbekannte Funktion: {functionCall.Name}"),
        };
    }

    private ISiteswapFilter CompileMinOcc(Argument[] args)
    {
        var numbers = GetNumbers(args[0]);
        var count = GetNumber(args[1]);
        return new FilterBuilder(input).MinimumOccurence(numbers, count).Build();
    }

    private ISiteswapFilter CompileMaxOcc(Argument[] args)
    {
        var numbers = GetNumbers(args[0]);
        var count = GetNumber(args[1]);
        return new FilterBuilder(input).MaximumOccurence(numbers, count).Build();
    }

    private ISiteswapFilter CompileExactOcc(Argument[] args)
    {
        var numbers = GetNumbers(args[0]);
        var count = GetNumber(args[1]);
        return new FilterBuilder(input).ExactOccurence(numbers, count).Build();
    }

    private ISiteswapFilter CompileOcc(Argument[] args)
    {
        var numbers = GetNumbers(args[0]);
        var min = GetNumber(args[1]);
        var max = GetNumber(args[2]);

        // occ(x, min, max) = minOcc(x, min) AND maxOcc(x, max)
        return new FilterBuilder(input)
            .MinimumOccurence(numbers, min)
            .MaximumOccurence(numbers, max)
            .Build();
    }

    private ISiteswapFilter CompilePattern(Argument[] args)
    {
        if (!numberOfJugglers.HasValue)
        {
            throw new InvalidOperationException("numberOfJugglers ist erforderlich für pattern()");
        }

        var patternValues = args.Select(GetPatternValue).ToList();
        return new FilterBuilder(input).Pattern(patternValues, numberOfJugglers.Value).Build();
    }

    private ISiteswapFilter CompileStartsWith(Argument[] args)
    {
        // Implementiert startsWith als pattern am Anfang
        var patternValues = args.Select(GetPatternValue).ToList();

        // Für startsWith: Prüfe die ersten n Positionen
        // Vereinfachte Implementierung: Verwende minOcc für jede Position
        IFilterBuilder builder = new FilterBuilder(input);
        foreach (var (value, index) in patternValues.Select((v, i) => (v, i)))
        {
            if (value >= 0)
            {
                builder = builder.MinimumOccurence([value], 1);
            }
        }
        return builder.Build();
    }

    private ISiteswapFilter CompileEndsWith(Argument[] args)
    {
        // Ähnlich wie startsWith
        var patternValues = args.Select(GetPatternValue).ToList();
        IFilterBuilder builder = new FilterBuilder(input);
        foreach (var value in patternValues.Where(v => v >= 0))
        {
            builder = builder.MinimumOccurence([value], 1);
        }
        return builder.Build();
    }

    private ISiteswapFilter CompileContains(Argument[] args)
    {
        // contains: Alle Werte müssen mindestens einmal vorkommen
        var patternValues = args.Select(GetPatternValue).ToList();
        IFilterBuilder builder = new FilterBuilder(input);
        foreach (var value in patternValues.Where(v => v >= 0))
        {
            builder = builder.MinimumOccurence([value], 1);
        }
        return builder.Build();
    }

    private ISiteswapFilter CompileHeight(Argument[] args)
    {
        var min = GetNumber(args[0]);
        var max = GetNumber(args[1]);

        // Kombiniere: kein Wurf unter min, kein Wurf über max
        IFilterBuilder builder = new FilterBuilder(input);
        for (var i = 0; i < min; i++)
        {
            builder = builder.MaximumOccurence([i], 0);
        }
        for (var i = max + 1; i <= input.MaxHeight; i++)
        {
            builder = builder.MaximumOccurence([i], 0);
        }
        return builder.Build();
    }

    private ISiteswapFilter CompileMaxHeight(Argument[] args)
    {
        var max = GetNumber(args[0]);
        IFilterBuilder builder = new FilterBuilder(input);
        for (var i = max + 1; i <= input.MaxHeight; i++)
        {
            builder = builder.MaximumOccurence([i], 0);
        }
        return builder.Build();
    }

    private ISiteswapFilter CompileMinHeight(Argument[] args)
    {
        var min = GetNumber(args[0]);
        IFilterBuilder builder = new FilterBuilder(input);
        for (var i = 0; i < min; i++)
        {
            builder = builder.MaximumOccurence([i], 0);
        }
        return builder.Build();
    }

    private ISiteswapFilter CompileOrbits(Argument[] args)
    {
        // Orbits-Filter - verwendet ggf. spezielle Logik
        // Für jetzt: Dummy-Implementierung
        return new FilterBuilder(input).No().Build();
    }

    private ISiteswapFilter CompilePasses(Argument[] args)
    {
        if (!numberOfJugglers.HasValue)
        {
            throw new InvalidOperationException("numberOfJugglers ist erforderlich für passes()");
        }

        var count = GetNumber(args[0]);
        return new FilterBuilder(input).ExactNumberOfPasses(count, numberOfJugglers.Value).Build();
    }

    private ISiteswapFilter CompileState(Argument[] args)
    {
        var stateValues = args.Select(a => GetNumber(a) == 1).ToList();
        var state = new State(stateValues);
        return new FilterBuilder(input).WithState(state).Build();
    }

    private ISiteswapFilter CompileInterface(Argument[] args)
    {
        var pattern = args[0]
            .Match(
                number => new List<List<int>> { new() { number.Value } },
                wildcard => new List<List<int>> { new() { -1 } },
                numberList => numberList.Values.Select(v => new List<int> { v }).ToList(),
                id => throw new InvalidOperationException("Identifier nicht erlaubt in Interface"),
                pass => new List<List<int>> { new() { -2 } },
                self => new List<List<int>> { new() { -3 } }
            );

        if (numberOfJugglers.HasValue)
        {
            var passValues = Enumerable
                .Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
                .Where(x => x % numberOfJugglers.Value != 0)
                .ToList();
            var selfValues = Enumerable
                .Range(input.MinHeight, input.MaxHeight - input.MinHeight + 1)
                .Where(x => x % numberOfJugglers.Value == 0)
                .ToList();

            pattern = pattern
                .Select(list =>
                    list.SelectMany(v =>
                            v switch
                            {
                                -2 => passValues,
                                -3 => selfValues,
                                _ => [v],
                            }
                        )
                        .ToList()
                )
                .ToList();
        }

        return new InterfaceFilter(pattern, numberOfJugglers ?? 2);
    }

    private ISiteswapFilter CompileGroundFilter()
    {
        // Ground State: State mit allen Positionen auf den niedrigsten Werten
        var groundState = new State(Enumerable.Repeat(true, input.NumberOfObjects).ToList());
        return new FilterBuilder(input).WithState(groundState).Build();
    }

    private ISiteswapFilter CompileExcitedFilter()
    {
        // Excited = NOT ground
        var groundFilter = CompileGroundFilter();
        return new FilterBuilder(input).Not(groundFilter).Build();
    }

    private ISiteswapFilter CompileNoZerosFilter()
    {
        return new FilterBuilder(input).MaximumOccurence([0], 0).Build();
    }

    private ISiteswapFilter CompileHasZerosFilter()
    {
        return new FilterBuilder(input).MinimumOccurence([0], 1).Build();
    }

    private static int GetNumber(Argument arg)
    {
        return arg.Match(
            number => number.Value,
            wildcard => throw new InvalidOperationException("Wildcard nicht erlaubt hier"),
            numberList => throw new InvalidOperationException("NumberList nicht erlaubt hier"),
            id => throw new InvalidOperationException("Identifier nicht erlaubt hier"),
            pass => throw new InvalidOperationException("Pass nicht erlaubt hier"),
            self => throw new InvalidOperationException("Self nicht erlaubt hier")
        );
    }

    private static IEnumerable<int> GetNumbers(Argument arg)
    {
        return arg.Match(
            number => new[] { number.Value },
            wildcard => throw new InvalidOperationException("Wildcard nicht erlaubt hier"),
            numberList => numberList.Values,
            id => throw new InvalidOperationException("Identifier nicht erlaubt hier"),
            pass => throw new InvalidOperationException("Pass nicht erlaubt hier"),
            self => throw new InvalidOperationException("Self nicht erlaubt hier")
        );
    }

    private static int GetPatternValue(Argument arg)
    {
        return arg.Match(
            number => number.Value,
            wildcard => -1, // -1 bedeutet "any" im Pattern
            numberList =>
                throw new InvalidOperationException("NumberList nicht erlaubt in Pattern"),
            id => throw new InvalidOperationException("Identifier nicht erlaubt in Pattern"),
            pass => -2, // -2 bedeutet "pass" (ungerade Zahl bei Passing)
            self => -3 // -3 bedeutet "self" (gerade Zahl bei Passing)
        );
    }
}
