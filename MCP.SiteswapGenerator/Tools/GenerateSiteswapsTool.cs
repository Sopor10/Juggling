using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;
using Siteswaps.Generator.Core.Generator.Filter.Combinatorics;

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
        [Description("Numbers that must occur at least this many times. Format: '3:2' for single, '3:2,4:1' for multiple, '3:1|4:1' for OR logic, '3,4:2' for multiple numbers")] string? minOccurrence = null,
        [Description("Numbers that must occur at most this many times. Format: '5:1' for single, '5:1,6:2' for multiple, '3,4:2' for multiple numbers")] string? maxOccurrence = null,
        [Description("Numbers that must occur exactly this many times. Format: '5:2' for single, '5:2,6:1' for multiple, '3,4:2' for multiple numbers")] string? exactOccurrence = null,
        [Description("Exact number of passes (requires numberOfJugglers)")] int? numberOfPasses = null,
        [Description("Number of jugglers (required for numberOfPasses/pattern)")] int? numberOfJugglers = null,
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
        
        // MinimumOccurrence Filter (unterstützt mehrere mit Komma, OR mit |)
        if (!string.IsNullOrWhiteSpace(minOccurrence))
        {
            // Prüfe auf OR-Logik (|)
            if (minOccurrence.Contains('|'))
            {
                var orFilters = new List<ISiteswapFilter>();
                foreach (var orPart in minOccurrence.Split('|'))
                {
                    IFilterBuilder tempBuilder = new FilterBuilder(input);
                    var parsed = ParseOccurrenceFiltersForBuilder(orPart.Trim());
                    foreach (var (numbers, amount) in parsed)
                    {
                        tempBuilder = tempBuilder.MinimumOccurence(numbers, amount);
                    }
                    var builtFilter = tempBuilder.Build();
                    orFilters.Add(builtFilter);
                }
                if (orFilters.Count > 0)
                {
                    // Or erwartet params IEnumerable<ISiteswapFilter>
                    // Erstelle einen temporären FilterBuilder für die OR-Logik
                    var orBuilder = new FilterBuilder(input);
                    filterBuilder = orBuilder.Or(orFilters);
                }
            }
            else
            {
                var parsed = ParseOccurrenceFiltersForBuilder(minOccurrence);
                foreach (var (numbers, amount) in parsed)
                {
                    filterBuilder = filterBuilder.MinimumOccurence(numbers, amount);
                }
            }
        }
        
        // MaximumOccurrence Filter (unterstützt mehrere mit Komma)
        if (!string.IsNullOrWhiteSpace(maxOccurrence))
        {
            var parsed = ParseOccurrenceFiltersForBuilder(maxOccurrence);
            foreach (var (numbers, amount) in parsed)
            {
                filterBuilder = filterBuilder.MaximumOccurence(numbers, amount);
            }
        }
        
        // ExactOccurrence Filter (unterstützt mehrere mit Komma)
        if (!string.IsNullOrWhiteSpace(exactOccurrence))
        {
            var parsed = ParseOccurrenceFiltersForBuilder(exactOccurrence);
            foreach (var (numbers, amount) in parsed)
            {
                filterBuilder = filterBuilder.ExactOccurence(numbers, amount);
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
    
    private static List<(IEnumerable<int> Numbers, int Amount)> ParseOccurrenceFiltersForBuilder(string occurrenceString)
    {
        var results = new List<(IEnumerable<int> Numbers, int Amount)>();
        
        // Unterstützt mehrere mit Komma getrennt: "3:2,4:1"
        foreach (var part in occurrenceString.Split(','))
        {
            var trimmed = part.Trim();
            var parts = trimmed.Split(':');
            
            if (parts.Length == 2 && int.TryParse(parts[0], out var number) && int.TryParse(parts[1], out var amount))
            {
                results.Add(([number], amount));
            }
            // Unterstützt auch mehrere Zahlen: "3,4:2" bedeutet 3 oder 4 mindestens 2x
            else if (parts.Length == 2)
            {
                var numbers = parts[0].Split(',')
                    .Select(s => s.Trim())
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .ToList();
                
                if (numbers.Any() && int.TryParse(parts[1], out var amountValue))
                {
                    results.Add((numbers, amountValue));
                }
            }
        }
        
        return results;
    }
}
