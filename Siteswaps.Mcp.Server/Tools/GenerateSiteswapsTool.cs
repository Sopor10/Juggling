using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class GenerateSiteswapsTool
{
    [McpServerTool]
    [Description("Generates siteswaps based on the specified parameters. Returns a list of siteswap patterns. Ask the user for missing information.")]
    public async Task<List<string>> GenerateSiteswaps(
        [Description("Period of the siteswap")] int period,
        [Description("Number of objects (balls)")] int numberOfObjects,
        [Description("Minimum throw height")] int minHeight,
        [Description("Maximum throw height")] int maxHeight,
        [Description("Maximum number of results to return")] int maxResults = 100,
        [Description("Timeout in seconds")] int timeoutSeconds = 30,
        [Description("Numbers that must occur at least this many times. Format: '3:2' for single, '3:2,4:1' for multiple, '3:1|4:1' for OR logic, '3,4:2' for multiple numbers")] string? minOccurrence = null,
        [Description("Numbers that must occur at most this many times. Format: '5:1' for single, '5:1,6:2' for multiple, '3,4:2' for multiple numbers")] string? maxOccurrence = null,
        [Description("Numbers that must occur exactly this many times. Format: '5:2' for single, '5:2,6:1' for multiple, '3,4:2' for multiple numbers")] string? exactOccurrence = null,
        [Description("Exact number of passes (requires numberOfJugglers)")] int? numberOfPasses = null,
        [Description("Number of jugglers (required for numberOfPasses/pattern)")] int? numberOfJugglers = null,
        [Description("Pattern to match (comma-separated numbers, e.g., '3,3,1')")] string? pattern = null,
        [Description("State filter (comma-separated 0/1 values, e.g., '1,1,0,0' for state with first two slots occupied)")] string? state = null,
        [Description("Flexible pattern (semicolon-separated groups, e.g., '3,4;5,6' for two groups)")] string? flexiblePattern = null,
        [Description("Use default filter (right amount of balls)")] bool useDefaultFilter = true,
        [Description("Use no filter (accepts all siteswaps)")] bool useNoFilter = false,
        [Description("Locally valid filter for specific juggler (requires numberOfJugglers and jugglerIndex)")] int? jugglerIndex = null,
        [Description("Rotation-aware flexible pattern for specific juggler (semicolon-separated groups, requires numberOfJugglers and jugglerIndex)")] string? rotationAwarePattern = null,
        [Description("Personalized number filter for specific juggler. Format: 'number:amount:type:from' where type is 'exact', 'atleast', or 'atmost' (requires numberOfJugglers)")] string? personalizedNumberFilter = null,
        [Description("Not filter - negate a filter. Format: 'minOccurrence:3:2' to negate minOccurrence filter, 'pattern:3,3,1' to negate pattern filter, etc. Use | for OR logic, e.g., 'minOccurrence:3:2|maxOccurrence:5:1'")] string? notFilter = null,
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
        var parser = new FilterParser(input, numberOfJugglers, minHeight, maxHeight);
        var filterBuilder = parser.BuildFilterFromParameters(
            minOccurrence: minOccurrence,
            maxOccurrence: maxOccurrence,
            exactOccurrence: exactOccurrence,
            numberOfPasses: numberOfPasses,
            pattern: pattern,
            state: state,
            flexiblePattern: flexiblePattern,
            useDefaultFilter: useDefaultFilter,
            useNoFilter: useNoFilter,
            jugglerIndex: jugglerIndex,
            rotationAwarePattern: rotationAwarePattern,
            personalizedNumberFilter: personalizedNumberFilter,
            notFilter: notFilter);
        
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
