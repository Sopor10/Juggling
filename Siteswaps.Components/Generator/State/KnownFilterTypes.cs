using System;
using System.Collections.Generic;
using System.Linq;
using Siteswaps.Components.Generator.Filter;

namespace Siteswaps.Components.Generator.State;

public class KnownFilterTypes
{
    public KnownFilterTypes()
    {
        Items = new()
        { 
            new(FilterType.Number, typeof(NumberFilter)),
            new(FilterType.Pattern, typeof(PatternFilter)),
        };
    }

    private List<FilterRendererMap> Items { get; }
    public Type? MapFilterInformationToRenderType(IFilterInformation filterInformation) => Items.FirstOrDefault(x => x.Key == filterInformation.FilterType)?.ViewType;

    public IEnumerable<FilterType> AvailableSelection() => Items.Select(x => x.Key).ToList();
    
}
public record FilterRendererMap(FilterType Key, Type ViewType);