using System.Collections.Immutable;

namespace Siteswaps.Generator.Components.State;

public record PatternFilterInformation : IFilterInformation
{
    public PatternFilterInformation(ImmutableArray<int> pattern)
    {
        Pattern = pattern;
    }

    public bool IsCompleted => false;
    public FilterType FilterType => FilterType.Pattern;
    public ImmutableArray<int> Pattern { get; init; }

    public string Display() => "Pattern";
}