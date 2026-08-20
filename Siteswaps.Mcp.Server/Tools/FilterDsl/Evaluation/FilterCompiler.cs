using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Mcp.Server.Tools.FilterDsl.Ast;

namespace Siteswaps.Mcp.Server.Tools.FilterDsl.Evaluation;

public class FilterCompiler(SiteswapGeneratorInput input, int? numberOfJugglers = null)
{
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
            _ => throw new InvalidOperationException($"Unbekannte Funktion: {functionCall.Name}"),
        };
    }

    private ISiteswapFilter CompileMinOcc(Argument[] args) => new FilterBuilder(input).MinimumOccurence(GetNumbers(args[0]), GetNumber(args[1])).Build();
    private ISiteswapFilter CompileMaxOcc(Argument[] args) => new FilterBuilder(input).MaximumOccurence(GetNumbers(args[0]), GetNumber(args[1])).Build();
    private ISiteswapFilter CompileExactOcc(Argument[] args) => new FilterBuilder(input).ExactOccurence(GetNumbers(args[0]), GetNumber(args[1])).Build();

    private ISiteswapFilter CompileOcc(Argument[] args) => new FilterBuilder(input)
        .MinimumOccurence(GetNumbers(args[0]), GetNumber(args[1]))
        .MaximumOccurence(GetNumbers(args[0]), GetNumber(args[2]))
        .Build();

    private ISiteswapFilter CompilePattern(Argument[] args)
    {
        if (!numberOfJugglers.HasValue) throw new InvalidOperationException("numberOfJugglers ist erforderlich für pattern()");
        return new FilterBuilder(input).Pattern(args.Select(GetPatternValue).ToList(), numberOfJugglers.Value).Build();
    }

    private ISiteswapFilter CompileStartsWith(Argument[] args) => CompileContains(args);
    private ISiteswapFilter CompileEndsWith(Argument[] args) => CompileContains(args);
    private ISiteswapFilter CompileContains(Argument[] args)
    {
        IFilterBuilder builder = new FilterBuilder(input);
        foreach (var value in args.Select(GetPatternValue).Where(v => v >= 0)) builder = builder.MinimumOccurence([value], 1);
        return builder.Build();
    }

    private ISiteswapFilter CompileHeight(Argument[] args)
    {
        var min = GetNumber(args[0]);
        var max = GetNumber(args[1]);
        IFilterBuilder builder = new FilterBuilder(input);
        for (var i = 0; i < min; i++) builder = builder.MaximumOccurence([i], 0);
        for (var i = max + 1; i <= input.MaxHeight; i++) builder = builder.MaximumOccurence([i], 0);
        return builder.Build();
    }

    private ISiteswapFilter CompileMaxHeight(Argument[] args)
    {
        IFilterBuilder builder = new FilterBuilder(input);
        for (var i = GetNumber(args[0]) + 1; i <= input.MaxHeight; i++) builder = builder.MaximumOccurence([i], 0);
        return builder.Build();
    }

    private ISiteswapFilter CompileMinHeight(Argument[] args)
    {
        IFilterBuilder builder = new FilterBuilder(input);
        for (var i = 0; i < GetNumber(args[0]); i++) builder = builder.MaximumOccurence([i], 0);
        return builder.Build();
    }

    private ISiteswapFilter CompileOrbits(Argument[] args) => new FilterBuilder(input).No().Build();

    private ISiteswapFilter CompilePasses(Argument[] args)
    {
        if (!numberOfJugglers.HasValue) throw new InvalidOperationException("numberOfJugglers ist erforderlich für passes()");
        return new FilterBuilder(input).ExactNumberOfPasses(GetNumber(args[0]), numberOfJugglers.Value).Build();
    }

    private ISiteswapFilter CompileState(Argument[] args) => new FilterBuilder(input).WithState(new State(args.Select(a => GetNumber(a) == 1).ToList())).Build();
    private ISiteswapFilter CompileGroundFilter() => new FilterBuilder(input).WithState(new State(Enumerable.Repeat(true, input.NumberOfObjects).ToList())).Build();
    private ISiteswapFilter CompileExcitedFilter() => new FilterBuilder(input).Not(CompileGroundFilter()).Build();
    private ISiteswapFilter CompileNoZerosFilter() => new FilterBuilder(input).MaximumOccurence([0], 0).Build();
    private ISiteswapFilter CompileHasZerosFilter() => new FilterBuilder(input).MinimumOccurence([0], 1).Build();

    private static int GetNumber(Argument arg) =>
        arg.Match(
            number => number.Value,
            wildcard => throw new InvalidOperationException("Wildcard nicht erlaubt hier"),
            numberList => throw new InvalidOperationException("NumberList nicht erlaubt hier"),
            id => throw new InvalidOperationException("Identifier nicht erlaubt hier"),
            pass => throw new InvalidOperationException("Pass nicht erlaubt hier"),
            self => throw new InvalidOperationException("Self nicht erlaubt hier")
        );

    private static int[] GetNumbers(Argument arg) =>
        arg.Match(
            number => new[] { number.Value },
            wildcard => throw new InvalidOperationException("Wildcard nicht erlaubt hier"),
            numberList => numberList.Values,
            id => throw new InvalidOperationException("Identifier nicht erlaubt hier"),
            pass => throw new InvalidOperationException("Pass nicht erlaubt hier"),
            self => throw new InvalidOperationException("Self nicht erlaubt hier")
        );

    private static int GetPatternValue(Argument arg) =>
        arg.Match(
            number => number.Value,
            wildcard => -1,
            numberList => throw new InvalidOperationException("NumberList nicht erlaubt in Pattern"),
            id => throw new InvalidOperationException("Identifier nicht erlaubt in Pattern"),
            pass => -2,
            self => -3
        );
}
