using ExhaustiveMatching;

namespace Siteswaps.Components.Generator.State;

public record GeneratorState
{
    public int NumberOfJugglers { get; init; }
    public Objects Objects => new ExactNumber();
    public int Period { get; init; } = 5;
    public int MaxThrow { get; init; } = 10;
    public int MinThrow { get; init; } = 2;
}

[Closed(typeof(ExactNumber), typeof(Between))]
public abstract record Objects;

public record ExactNumber : Objects
{
    public int Number { get; init; } = 7;
}

public record Between : Objects
{
    public int MinNumber { get; init; } = 6;
    public int MaxNumber { get; init; } = 7;
}
