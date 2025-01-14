namespace Siteswaps.Generator.Generator;

public record SiteswapGeneratorInput
{
    public SiteswapGeneratorInput()
    {
        Period = 5;
        MaxHeight = 10;
        MinHeight = 2;
        NumberOfObjects = 7;
    }

    public SiteswapGeneratorInput(int period, int numberOfObjects, int minHeight, int maxHeight)
    {
        Period = period;
        NumberOfObjects = numberOfObjects;
        MinHeight = minHeight;
        MaxHeight = maxHeight;
    }

    public int NumberOfObjects { get; init; } = 7;
    public int Period { get; init; } = 5;
    public int MinHeight { get; init; } = 2;
    public int MaxHeight { get; init; } = 10;
    public StopCriteria StopCriteria { get; init; } = new(TimeSpan.FromSeconds(15), 1000);
}

public record StopCriteria(TimeSpan TimeOut, int MaxNumberOfResults);
