﻿using System.Collections.Immutable;

namespace Siteswaps.Generator.Components.State;

public record PatternFilterInformation(ImmutableArray<int> Pattern) : IFilterInformation
{
    public bool IsCompleted => false;
    public FilterType FilterType => FilterType.Pattern;

    public string Display() => "include " + string.Join(" ", Pattern.Select(ToDisplay).ToList());

    private string ToDisplay(int i) => 
        i switch
        {
            -3 => "s",
            -2 => "p",
            -1 => "_",
            var x => x.ToString(),
        };
}