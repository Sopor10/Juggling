using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Mcp.Server.Tools.FilterDsl;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class GenerateSiteswapsTool
{
    [McpServerTool]
    [Description(
        "Generates siteswaps based on the specified parameters. Returns a list of siteswap patterns. "
            + "Use the 'filter' parameter with DSL syntax for powerful filtering. "
            + "Examples: 'minOcc(5,2) AND ground', 'pattern(5,*,1) OR pattern(7,*,1)', 'NOT hasZeros AND minOcc(7,1)'. "
            + "Available functions: minOcc, maxOcc, exactOcc, pattern, contains, startsWith, endsWith, passes, state, height, maxHeight, minHeight. "
            + "Keywords: ground, excited, noZeros, hasZeros. "
            + "Operators: AND, OR, NOT (with parentheses for grouping)."
            + "More details can be found in the resources of this MCP server."
    )]
    public async Task<List<string>> GenerateSiteswaps(
        [Description("Period of the siteswap")] int period,
        [Description("Number of objects (balls)")] int numberOfObjects,
        [Description("Minimum throw height")] int minHeight,
        [Description("Maximum throw height")] int maxHeight,
        [Description("Maximum number of results to return")] int maxResults = 100,
        [Description("Timeout in seconds")] int timeoutSeconds = 30,
        [Description(
            "Filter expression using DSL syntax. "
                + "Examples: 'minOcc(5,2)', 'ground AND noZeros', '(minOcc(7,2) OR exactOcc(9,1)) AND ground', 'pattern(5,*,1)'. "
                + "Functions: minOcc(throw,count), maxOcc(throw,count), exactOcc(throw,count), pattern(...), contains(...), passes(count), state(...), height(min,max), maxHeight(max), minHeight(min). "
                + "Keywords (no args): ground, excited, noZeros, hasZeros. "
                + "Operators: AND, OR, NOT. Use parentheses for grouping. Wildcards (*) in pattern for 'any throw'."
        )]
            string? filter = null,
        [Description("Number of jugglers (required for pattern/passes functions)")]
            int? numberOfJugglers = null,
        CancellationToken cancellationToken = default
    )
    {
        // SiteswapGeneratorInput erstellen
        var input = new SiteswapGeneratorInput(
            period: period,
            numberOfObjects: numberOfObjects,
            minHeight: minHeight,
            maxHeight: maxHeight
        )
        {
            StopCriteria = new StopCriteria(TimeSpan.FromSeconds(timeoutSeconds), maxResults),
        };

        ISiteswapFilter siteswapFilter;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var dslParser = new FilterDslParser(input, numberOfJugglers);
            var dslResult = dslParser.CreateFilter(filter);

            if (!dslResult.Success)
            {
                throw new ArgumentException($"Filter-DSL Fehler: {dslResult.ErrorMessage}");
            }

            var defaultFilter = new FilterBuilder(input).WithDefault().Build();
            siteswapFilter = new FilterBuilder(input).And(dslResult.Filter!, defaultFilter).Build();
        }
        else
        {
            // Kein Filter angegeben - nur Default-Filter verwenden
            siteswapFilter = new FilterBuilder(input).WithDefault().Build();
        }

        // SiteswapGenerator mit Filter erstellen und ausführen
        var generator = new SiteswapGenerator(siteswapFilter, input);

        var results = new List<string>();
        await foreach (var siteswap in generator.GenerateAsync(cancellationToken))
        {
            results.Add(siteswap.ToString());
        }

        return results;
    }
}
