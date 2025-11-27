using System.ComponentModel;
using System.Runtime.CompilerServices;
using ModelContextProtocol.Server;
using Siteswaps.Generator.Core.Generator;

namespace MCP.SiteswapGenerator.Tools;

public static class GenerateSiteswapsTool
{
    [McpServerTool]
    [Description("Generates siteswaps based on the specified parameters. Returns a stream of siteswap patterns.")]
    public static async IAsyncEnumerable<string> GenerateSiteswaps(
        [Description("Period of the siteswap")] int period,
        [Description("Number of objects (balls)")] int numberOfObjects,
        [Description("Minimum throw height")] int minHeight,
        [Description("Maximum throw height")] int maxHeight,
        [Description("Maximum number of results to return")] int maxResults = 100,
        [Description("Timeout in seconds")] int timeoutSeconds = 30,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

        await foreach (var siteswap in generator.GenerateAsync(cancellationToken))
        {
            yield return siteswap.ToString();
        }
    }
}

