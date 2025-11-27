using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace MCP.SiteswapGenerator.Tools;

public class GenerateSiteswapsTool
{
    [McpServerTool]
    [Description("Generates siteswaps based on the specified parameters. Returns a list of siteswap patterns.")]
    public async Task<List<string>> GenerateSiteswaps(
        [Description("Period of the siteswap")] int period,
        [Description("Number of objects (balls)")] int numberOfObjects,
        [Description("Minimum throw height")] int minHeight,
        [Description("Maximum throw height")] int maxHeight,
        [Description("Maximum number of results to return")] int maxResults = 100,
        [Description("Timeout in seconds")] int timeoutSeconds = 30,
        [Description("Numbers that must occur at least this many times (e.g., '3:2' means number 3 at least 2 times)")] string? minOccurrence = null,
        [Description("Numbers that must occur at most this many times (e.g., '5:1' means number 5 at most 1 time)")] string? maxOccurrence = null,
        [Description("Exact number of passes (requires numberOfJugglers)")] int? numberOfPasses = null,
        [Description("Number of jugglers (required for numberOfPasses)")] int? numberOfJugglers = null,
        [Description("Pattern to match (comma-separated numbers, e.g., '3,3,1')")] string? pattern = null,
        CancellationToken cancellationToken = default)
    {
        // SiteswapGeneratorInput erstellen
        var input = new SiteswapGeneratorInput(
            period: period,
            numberOfObjects: numberOfObjects,
            minHeight: minHeight,
            maxHeight: maxHeight
        )
        {
            StopCriteria = new StopCriteria(
                TimeSpan.FromSeconds(timeoutSeconds),
                maxResults
            )
        };

        // Filter erstellen
        IFilterBuilder filterBuilder = new FilterBuilder(input);
        
        // MinimumOccurrence Filter
        if (!string.IsNullOrWhiteSpace(minOccurrence))
        {
            var parts = minOccurrence.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[0], out var number) && int.TryParse(parts[1], out var amount))
            {
                filterBuilder = filterBuilder.MinimumOccurence([number], amount);
            }
        }
        
        // MaximumOccurrence Filter
        if (!string.IsNullOrWhiteSpace(maxOccurrence))
        {
            var parts = maxOccurrence.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[0], out var number) && int.TryParse(parts[1], out var amount))
            {
                filterBuilder = filterBuilder.MaximumOccurence([number], amount);
            }
        }
        
        // NumberOfPasses Filter
        if (numberOfPasses.HasValue && numberOfJugglers.HasValue)
        {
            filterBuilder = filterBuilder.ExactNumberOfPasses(numberOfPasses.Value, numberOfJugglers.Value);
        }
        
        // Pattern Filter
        if (!string.IsNullOrWhiteSpace(pattern))
        {
            var patternNumbers = pattern.Split(',')
                .Select(s => s.Trim())
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse)
                .ToList();
            
            if (patternNumbers.Any() && numberOfJugglers.HasValue)
            {
                filterBuilder = filterBuilder.Pattern(patternNumbers, numberOfJugglers.Value);
            }
        }
        
        var filter = filterBuilder.Build();

        // SiteswapGenerator mit Filter erstellen und ausführen
        var generator = new Siteswaps.Generator.Core.Generator.SiteswapGenerator(filter, input);

        var results = new List<string>();
        await foreach (var siteswap in generator.GenerateAsync(cancellationToken))
        {
            results.Add(siteswap.ToString());
        }

        return results;
    }
}

