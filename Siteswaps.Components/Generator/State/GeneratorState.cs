using ExhaustiveMatching;

namespace Siteswaps.Components.Generator.State;

public record GeneratorState
{
    public bool IsExactNumber => Objects is ExactNumber;
    public int NumberOfJugglers { get; init; } = 2;
    public Objects Objects { get; init; } = new ExactNumber();
    public int Period { get; init; } = 5;
    public int MaxThrow { get; init; } = 10;
    public int MinThrow { get; init; } = 2;
    public bool IsGenerating { get; init; } = false;
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
