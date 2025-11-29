using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

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
        
        // MaximumOccurrence Filter (unterstützt mehrere mit Komma, OR mit |)
        if (!string.IsNullOrWhiteSpace(maxOccurrence))
        {
            // Prüfe auf OR-Logik (|)
            if (maxOccurrence.Contains('|'))
            {
                var orFilters = new List<ISiteswapFilter>();
                foreach (var orPart in maxOccurrence.Split('|'))
                {
                    IFilterBuilder tempBuilder = new FilterBuilder(input);
                    var parsed = ParseOccurrenceFiltersForBuilder(orPart.Trim());
                    foreach (var (numbers, amount) in parsed)
                    {
                        tempBuilder = tempBuilder.MaximumOccurence(numbers, amount);
                    }
                    var builtFilter = tempBuilder.Build();
                    orFilters.Add(builtFilter);
                }
                if (orFilters.Count > 0)
                {
                    var orBuilder = new FilterBuilder(input);
                    filterBuilder = orBuilder.Or(orFilters);
                }
            }
            else
            {
                var parsed = ParseOccurrenceFiltersForBuilder(maxOccurrence);
                foreach (var (numbers, amount) in parsed)
                {
                    filterBuilder = filterBuilder.MaximumOccurence(numbers, amount);
                }
            }
        }
        
        // ExactOccurrence Filter (unterstützt mehrere mit Komma, OR mit |)
        if (!string.IsNullOrWhiteSpace(exactOccurrence))
        {
            // Prüfe auf OR-Logik (|)
            if (exactOccurrence.Contains('|'))
            {
                var orFilters = new List<ISiteswapFilter>();
                foreach (var orPart in exactOccurrence.Split('|'))
                {
                    IFilterBuilder tempBuilder = new FilterBuilder(input);
                    var parsed = ParseOccurrenceFiltersForBuilder(orPart.Trim());
                    foreach (var (numbers, amount) in parsed)
                    {
                        tempBuilder = tempBuilder.ExactOccurence(numbers, amount);
                    }
                    var builtFilter = tempBuilder.Build();
                    orFilters.Add(builtFilter);
                }
                if (orFilters.Count > 0)
                {
                    var orBuilder = new FilterBuilder(input);
                    filterBuilder = orBuilder.Or(orFilters);
                }
            }
            else
            {
                var parsed = ParseOccurrenceFiltersForBuilder(exactOccurrence);
                foreach (var (numbers, amount) in parsed)
                {
                    filterBuilder = filterBuilder.ExactOccurence(numbers, amount);
                }
            }
        }
        
        // NumberOfPasses Filter
        if (numberOfPasses.HasValue && numberOfJugglers.HasValue)
        {
            filterBuilder = filterBuilder.ExactNumberOfPasses(numberOfPasses.Value, numberOfJugglers.Value);
        }
        
        // Pattern Filter (unterstützt OR mit |)
        if (!string.IsNullOrWhiteSpace(pattern))
        {
            // Prüfe auf OR-Logik (|)
            if (pattern.Contains('|'))
            {
                var orFilters = new List<ISiteswapFilter>();
                foreach (var orPart in pattern.Split('|'))
                {
                    var patternNumbers = orPart.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse)
                        .ToList();
                    
                    if (patternNumbers.Any() && numberOfJugglers.HasValue)
                    {
                        IFilterBuilder tempBuilder = new FilterBuilder(input);
                        tempBuilder = tempBuilder.Pattern(patternNumbers, numberOfJugglers.Value);
                        var builtFilter = tempBuilder.Build();
                        orFilters.Add(builtFilter);
                    }
                }
                if (orFilters.Count > 0)
                {
                    var orBuilder = new FilterBuilder(input);
                    filterBuilder = orBuilder.Or(orFilters);
                }
            }
            else
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
        }
        
        // State Filter (unterstützt OR mit |)
        if (!string.IsNullOrWhiteSpace(state))
        {
            // Prüfe auf OR-Logik (|)
            if (state.Contains('|'))
            {
                var orFilters = new List<ISiteswapFilter>();
                foreach (var orPart in state.Split('|'))
                {
                    var stateValues = orPart.Split(',')
                        .Select(s => s.Trim())
                        .Select(s => s == "1" || s.ToLower() == "true")
                        .ToList();
                    
                    if (stateValues.Any())
                    {
                        var stateObj = new State(stateValues);
                        IFilterBuilder tempBuilder = new FilterBuilder(input);
                        tempBuilder = tempBuilder.WithState(stateObj);
                        var builtFilter = tempBuilder.Build();
                        orFilters.Add(builtFilter);
                    }
                }
                if (orFilters.Count > 0)
                {
                    var orBuilder = new FilterBuilder(input);
                    filterBuilder = orBuilder.Or(orFilters);
                }
            }
            else
            {
                var stateValues = state.Split(',')
                    .Select(s => s.Trim())
                    .Select(s => s == "1" || s.ToLower() == "true")
                    .ToList();
                
                if (stateValues.Any())
                {
                    var stateObj = new State(stateValues);
                    filterBuilder = filterBuilder.WithState(stateObj);
                }
            }
        }
        
        // Flexible Pattern Filter (unterstützt OR mit |)
        if (!string.IsNullOrWhiteSpace(flexiblePattern) && numberOfJugglers.HasValue)
        {
            // Prüfe auf OR-Logik (|)
            if (flexiblePattern.Contains('|'))
            {
                var orFilters = new List<ISiteswapFilter>();
                foreach (var orPart in flexiblePattern.Split('|'))
                {
                    var groups = orPart.Split(';')
                        .Select(group => group.Split(',')
                            .Select(s => s.Trim())
                            .Where(s => int.TryParse(s, out _))
                            .Select(int.Parse)
                            .ToList())
                        .Where(g => g.Any())
                        .ToList();
                    
                    if (groups.Any())
                    {
                        IFilterBuilder tempBuilder = new FilterBuilder(input);
                        tempBuilder = tempBuilder.FlexiblePattern(groups, numberOfJugglers.Value, isGlobalPattern: true);
                        var builtFilter = tempBuilder.Build();
                        orFilters.Add(builtFilter);
                    }
                }
                if (orFilters.Count > 0)
                {
                    var orBuilder = new FilterBuilder(input);
                    filterBuilder = orBuilder.Or(orFilters);
                }
            }
            else
            {
                var groups = flexiblePattern.Split(';')
                    .Select(group => group.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse)
                        .ToList())
                    .Where(g => g.Any())
                    .ToList();
                
                if (groups.Any())
                {
                    filterBuilder = filterBuilder.FlexiblePattern(groups, numberOfJugglers.Value, isGlobalPattern: true);
                }
            }
        }
        
        // NoFilter (akzeptiert alles)
        if (useNoFilter)
        {
            filterBuilder = filterBuilder.No();
        }
        
        // LocallyValidFilter (für spezifischen Jongleur)
        if (jugglerIndex.HasValue && numberOfJugglers.HasValue)
        {
            var locallyValidFilter = new LocallyValidFilter(numberOfJugglers.Value, jugglerIndex.Value);
            filterBuilder = filterBuilder.And([locallyValidFilter]);
        }
        
        // RotationAwareFlexiblePatternFilter (für spezifischen Jongleur)
        if (!string.IsNullOrWhiteSpace(rotationAwarePattern) && numberOfJugglers.HasValue && jugglerIndex.HasValue)
        {
            var groups = rotationAwarePattern.Split(';')
                .Select(group => group.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .ToList())
                .Where(g => g.Any())
                .ToList();
            
            if (groups.Any())
            {
                var rotationAwareFilter = new RotationAwareFlexiblePatternFilter(
                    groups,
                    numberOfJugglers.Value,
                    input,
                    jugglerIndex.Value
                );
                filterBuilder = filterBuilder.And([rotationAwareFilter]);
            }
        }
        
        // PersonalizedNumberFilter (für spezifischen Jongleur)
        if (!string.IsNullOrWhiteSpace(personalizedNumberFilter) && numberOfJugglers.HasValue)
        {
            var parts = personalizedNumberFilter.Split(':');
            if (parts.Length == 4)
            {
                var numberParts = parts[0].Split(',')
                    .Select(s => s.Trim())
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .ToList();
                
                if (numberParts.Any() && 
                    int.TryParse(parts[1], out var amount) &&
                    Enum.TryParse<PersonalizedNumberFilter.Type>(parts[2], true, out var type) &&
                    int.TryParse(parts[3], out var from))
                {
                    var personalizedFilter = new PersonalizedNumberFilter(
                        numberOfJugglers.Value,
                        minHeight,
                        maxHeight,
                        numberParts,
                        amount,
                        type,
                        from
                    );
                    filterBuilder = filterBuilder.And([personalizedFilter]);
                }
            }
        }
        
        // Default Filter (RightAmountOfBallsFilter)
        if (useDefaultFilter)
        {
            filterBuilder = filterBuilder.WithDefault();
        }
        
        // Not Filter (negate a filter)
        if (!string.IsNullOrWhiteSpace(notFilter))
        {
            var notFilterObj = ParseAndBuildNotFilter(notFilter, input, numberOfJugglers, minHeight, maxHeight);
            if (notFilterObj != null)
            {
                filterBuilder = filterBuilder.Not(notFilterObj);
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
    
    private static ISiteswapFilter? ParseAndBuildNotFilter(
        string notFilterString,
        SiteswapGeneratorInput input,
        int? numberOfJugglers,
        int minHeight,
        int maxHeight)
    {
        // Format: 'filterType:value' oder 'filterType:value|filterType:value' für OR
        // Beispiele: 'minOccurrence:3:2', 'pattern:3,3,1', 'state:1,1,0,0'
        
        // Prüfe auf OR-Logik (|)
        if (notFilterString.Contains('|'))
        {
            var orFilters = new List<ISiteswapFilter>();
            foreach (var orPart in notFilterString.Split('|'))
            {
                var filter = ParseSingleNotFilter(orPart.Trim(), input, numberOfJugglers, minHeight, maxHeight);
                if (filter != null)
                {
                    orFilters.Add(filter);
                }
            }
            if (orFilters.Count > 0)
            {
                var orBuilder = new FilterBuilder(input);
                return orBuilder.Or(orFilters).Build();
            }
        }
        else
        {
            return ParseSingleNotFilter(notFilterString, input, numberOfJugglers, minHeight, maxHeight);
        }
        
        return null;
    }
    
    private static ISiteswapFilter? ParseSingleNotFilter(
        string notFilterString,
        SiteswapGeneratorInput input,
        int? numberOfJugglers,
        int minHeight,
        int maxHeight)
    {
        var parts = notFilterString.Split(':', 2);
        if (parts.Length != 2)
            return null;
        
        var filterType = parts[0].Trim().ToLower();
        var filterValue = parts[1].Trim();
        
        IFilterBuilder tempBuilder = new FilterBuilder(input);
        
        switch (filterType)
        {
            case "minoccurrence":
            case "min_occurrence":
                var minParsed = ParseOccurrenceFiltersForBuilder(filterValue);
                foreach (var (numbers, amount) in minParsed)
                {
                    tempBuilder = tempBuilder.MinimumOccurence(numbers, amount);
                }
                return tempBuilder.Build();
                
            case "maxoccurrence":
            case "max_occurrence":
                var maxParsed = ParseOccurrenceFiltersForBuilder(filterValue);
                foreach (var (numbers, amount) in maxParsed)
                {
                    tempBuilder = tempBuilder.MaximumOccurence(numbers, amount);
                }
                return tempBuilder.Build();
                
            case "exactoccurrence":
            case "exact_occurrence":
                var exactParsed = ParseOccurrenceFiltersForBuilder(filterValue);
                foreach (var (numbers, amount) in exactParsed)
                {
                    tempBuilder = tempBuilder.ExactOccurence(numbers, amount);
                }
                return tempBuilder.Build();
                
            case "pattern":
                if (numberOfJugglers.HasValue)
                {
                    var patternNumbers = filterValue.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse)
                        .ToList();
                    
                    if (patternNumbers.Any())
                    {
                        tempBuilder = tempBuilder.Pattern(patternNumbers, numberOfJugglers.Value);
                        return tempBuilder.Build();
                    }
                }
                break;
                
            case "state":
                var stateValues = filterValue.Split(',')
                    .Select(s => s.Trim())
                    .Select(s => s == "1" || s.ToLower() == "true")
                    .ToList();
                
                if (stateValues.Any())
                {
                    var stateObj = new State(stateValues);
                    tempBuilder = tempBuilder.WithState(stateObj);
                    return tempBuilder.Build();
                }
                break;
                
            case "flexiblepattern":
            case "flexible_pattern":
                if (numberOfJugglers.HasValue)
                {
                    var groups = filterValue.Split(';')
                        .Select(group => group.Split(',')
                            .Select(s => s.Trim())
                            .Where(s => int.TryParse(s, out _))
                            .Select(int.Parse)
                            .ToList())
                        .Where(g => g.Any())
                        .ToList();
                    
                    if (groups.Any())
                    {
                        tempBuilder = tempBuilder.FlexiblePattern(groups, numberOfJugglers.Value, isGlobalPattern: true);
                        return tempBuilder.Build();
                    }
                }
                break;
                
            case "numberofpasses":
            case "number_of_passes":
                if (numberOfJugglers.HasValue && int.TryParse(filterValue, out var passes))
                {
                    tempBuilder = tempBuilder.ExactNumberOfPasses(passes, numberOfJugglers.Value);
                    return tempBuilder.Build();
                }
                break;
        }
        
        return null;
    }
}
