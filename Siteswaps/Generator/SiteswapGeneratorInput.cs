using System;
using Siteswaps.Generator.Filter;

namespace Siteswaps.Generator;

public record SiteswapGeneratorInput
{
    public SiteswapGeneratorInput()
    {
        Filter = new FilterFactory().Standard();
        Period = 5;
        MaxHeight = 10;
        MinHeight = 2;
        NumberOfObjects = 7;
    }
    public SiteswapGeneratorInput(int period, int numberOfObjects, int minHeight, int maxHeight, ISiteswapFilter filter)
    {
        Period = period;
        NumberOfObjects = numberOfObjects;
        MinHeight = minHeight;
        MaxHeight = maxHeight;
        Filter = filter;
    }
    public SiteswapGeneratorInput(int period, int numberOfObjects, int minHeight, int maxHeight)
    {
        Period = period;
        NumberOfObjects = numberOfObjects;
        MinHeight = minHeight;
        MaxHeight = maxHeight;
        Filter = new NoFilter();
    }

    public int NumberOfObjects { get; init; } = 7;
    public int Period { get; init; } = 5;
    public int MinHeight { get; init; } = 2;
    public int MaxHeight { get; init; } = 10;
    public ISiteswapFilter Filter { get; init; } = new FilterFactory().Standard();
}
public record StopCriteria(TimeSpan TimeOut, int MaxNumberOfResults);