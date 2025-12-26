using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Generator.Core.Generator.Filter.FilterDsl;

namespace Siteswaps.Generator.Cli.Commands;

[Command("generate", Description = "Generates siteswaps based on the specified parameters.")]
public class GenerateCommand : ICommand
{
    [CommandOption("period", 'p', Description = "The period of the siteswap.")]
    public int Period { get; init; } = 5;

    [CommandOption("objects", 'o', Description = "The number of objects.")]
    public int NumberOfObjects { get; init; } = 3;
    
    [CommandOption("jugglers", 'j', Description = "The number of jugglers.")]
    public int NumberOfJugglers { get; init; } = 2;

    [CommandOption("min", Description = "The minimum throw height.")]
    public int MinHeight { get; init; } = 2;

    [CommandOption("max", Description = "The maximum throw height.")]
    public int MaxHeight { get; init; } = 10;

    [CommandOption("max-results", 'n', Description = "The maximum number of results.")]
    public int MaxResults { get; init; } = 100;

    [CommandOption("filter", 'f', Description = "Filter expression in DSL format.")]
    public string? Filter { get; init; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var input = new SiteswapGeneratorInput
        {
            Period = Period,
            NumberOfObjects = NumberOfObjects,
            MinHeight = MinHeight,
            MaxHeight = MaxHeight,
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(15), MaxResults),
        };

        ISiteswapFilter filter = new NoFilter();

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            var parser = new FilterDslParser(input, numberOfJugglers: NumberOfJugglers);
            var result = parser.CreateFilter(Filter);

            if (!result.Success)
            {
                await console.Error.WriteLineAsync($"Invalid filter: {result.ErrorMessage}");
                return;
            }

            filter = result.Filter!;
        }

        var generator = new SiteswapGenerator(filter, input);

        await foreach (
            var siteswap in generator.GenerateAsync(console.RegisterCancellationHandler())
        )
        {
            await console.Output.WriteLineAsync(siteswap.ToString());
        }
    }
}
