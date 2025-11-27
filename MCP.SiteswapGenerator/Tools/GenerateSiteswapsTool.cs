using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswaps.Generator.Core.Generator;

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

        // SiteswapGenerator erstellen und ausführen
        var generator = new Siteswaps.Generator.Core.Generator.SiteswapGenerator(input);

        var results = new List<string>();
        await foreach (var siteswap in generator.GenerateAsync(cancellationToken))
        {
            results.Add(siteswap.ToString());
        }

        return results;
    }
}

