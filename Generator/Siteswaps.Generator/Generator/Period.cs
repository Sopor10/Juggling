namespace Siteswaps.Generator.Generator;

public record Period(int Value)
{
    public LocalPeriod GetLocalPeriod(int numberOfJugglers) =>
        Value % numberOfJugglers == 0 ? new(Value / numberOfJugglers) : new(Value);
}
