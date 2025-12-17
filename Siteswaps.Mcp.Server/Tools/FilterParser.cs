using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;
using Siteswaps.Generator.Core.Generator.Filter.NumberFilter;

namespace Siteswaps.Mcp.Server.Tools;

public class FilterParser(
    SiteswapGeneratorInput input,
    int? numberOfJugglers = null,
    int minHeight = 0,
    int maxHeight = 0
)
{
    /// <summary>
    /// Parst Occurrence-Filter-Strings (minOccurrence, maxOccurrence, exactOccurrence)
    /// Format: '3:2' für einzelne, '3:2,4:1' für mehrere, '3,4:2' für mehrere Zahlen
    /// </summary>
    public List<(IEnumerable<int> Numbers, int Amount)> ParseOccurrenceFilters(
        string occurrenceString
    )
    {
        var results = new List<(IEnumerable<int> Numbers, int Amount)>();

        // Unterstützt mehrere mit Komma getrennt: "3:2,4:1"
        // Wichtig: Wir müssen zuerst nach Doppelpunkt suchen, um "3,4:2" korrekt zu parsen
        // Split nach Komma, aber nur wenn kein Doppelpunkt davor kommt
        var parts = new List<string>();
        var currentPart = new System.Text.StringBuilder();

        foreach (var c in occurrenceString)
        {
            if (c == ',' && currentPart.ToString().Contains(':'))
            {
                // Komma nach Doppelpunkt = neuer Eintrag
                parts.Add(currentPart.ToString().Trim());
                currentPart.Clear();
            }
            else
            {
                currentPart.Append(c);
            }
        }
        if (currentPart.Length > 0)
        {
            parts.Add(currentPart.ToString().Trim());
        }

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            var splitParts = trimmed.Split(':');

            if (splitParts.Length == 2)
            {
                // Unterstützt einzelne Zahl: "3:2"
                if (
                    int.TryParse(splitParts[0], out var number)
                    && int.TryParse(splitParts[1], out var amount)
                )
                {
                    results.Add(([number], amount));
                }
                // Unterstützt auch mehrere Zahlen: "3,4:2" bedeutet 3 oder 4 mindestens 2x
                else
                {
                    var numbers = splitParts[0]
                        .Split(',')
                        .Select(s => s.Trim())
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse)
                        .ToList();

                    if (numbers.Any() && int.TryParse(splitParts[1], out var amountValue))
                    {
                        results.Add((numbers, amountValue));
                    }
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Parst Pattern-Strings (Komma-getrennte Zahlen)
    /// Format: '3,3,1' oder '3,-1,1' für negative Zahlen
    /// Unterstützt negative Zahlen: -1 (Empty/Egal), -2 (AnySelf/P), -3 (AnyPass/S)
    /// </summary>
    public List<int>? ParsePattern(string patternString)
    {
        var patternNumbers = patternString
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .ToList();

        return patternNumbers.Any() ? patternNumbers : null;
    }

    /// <summary>
    /// Parst State-Strings (Komma-getrennte 0/1 Werte)
    /// Format: '1,1,0,0'
    /// </summary>
    public State? ParseState(string stateString)
    {
        if (string.IsNullOrWhiteSpace(stateString))
            return null;

        var stateValues = stateString
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s == "1" || s.ToLower() == "true")
            .ToList();

        return stateValues.Any() ? new State(stateValues) : null;
    }

    /// <summary>
    /// Parst FlexiblePattern-Strings (Semikolon-getrennte Gruppen, Komma-getrennte Zahlen)
    /// Format: '3,4;5,6'
    /// </summary>
    public List<List<int>>? ParseFlexiblePattern(string flexiblePatternString)
    {
        var groups = flexiblePatternString
            .Split(';')
            .Select(group =>
                group
                    .Split(',')
                    .Select(s => s.Trim())
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .ToList()
            )
            .Where(g => g.Any())
            .ToList();

        return groups.Any() ? groups : null;
    }

    /// <summary>
    /// Parst PersonalizedNumberFilter-Strings
    /// Format: 'number:amount:type:from' wobei type 'exact', 'atleast', oder 'atmost' ist
    /// </summary>
    public PersonalizedNumberFilter? ParsePersonalizedNumberFilter(
        string personalizedNumberFilterString
    )
    {
        var parts = personalizedNumberFilterString.Split(':');
        if (parts.Length != 4 || !numberOfJugglers.HasValue)
            return null;

        var numberParts = parts[0]
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .ToList();

        if (
            numberParts.Any()
            && int.TryParse(parts[1], out var amount)
            && Enum.TryParse<PersonalizedNumberFilter.Type>(parts[2], true, out var type)
            && int.TryParse(parts[3], out var from)
        )
        {
            return new PersonalizedNumberFilter(
                numberOfJugglers.Value,
                minHeight,
                maxHeight,
                numberParts,
                amount,
                type,
                from
            );
        }

        return null;
    }

    /// <summary>
    /// Parst Not-Filter-Strings und erstellt den entsprechenden Filter
    /// Format: 'filterType:value' oder 'filterType:value|filterType:value' für OR
    /// Beispiele: 'minOccurrence:3:2', 'pattern:3,3,1', 'state:1,1,0,0'
    /// </summary>
    public ISiteswapFilter? ParseAndBuildNotFilter(string notFilterString)
    {
        // Prüfe auf OR-Logik (|)
        if (notFilterString.Contains('|'))
        {
            var orFilters = new List<ISiteswapFilter>();
            foreach (var orPart in notFilterString.Split('|'))
            {
                var filter = ParseSingleNotFilter(orPart.Trim());
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
            return ParseSingleNotFilter(notFilterString);
        }

        return null;
    }

    /// <summary>
    /// Parst einen einzelnen Not-Filter-String
    /// </summary>
    private ISiteswapFilter? ParseSingleNotFilter(string notFilterString)
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
                var minParsed = ParseOccurrenceFilters(filterValue);
                foreach (var (numbers, amount) in minParsed)
                {
                    tempBuilder = tempBuilder.MinimumOccurence(numbers, amount);
                }
                return tempBuilder.Build();

            case "maxoccurrence":
            case "max_occurrence":
                var maxParsed = ParseOccurrenceFilters(filterValue);
                foreach (var (numbers, amount) in maxParsed)
                {
                    tempBuilder = tempBuilder.MaximumOccurence(numbers, amount);
                }
                return tempBuilder.Build();

            case "exactoccurrence":
            case "exact_occurrence":
                var exactParsed = ParseOccurrenceFilters(filterValue);
                foreach (var (numbers, amount) in exactParsed)
                {
                    tempBuilder = tempBuilder.ExactOccurence(numbers, amount);
                }
                return tempBuilder.Build();

            case "pattern":
                if (numberOfJugglers.HasValue)
                {
                    var patternNumbers = ParsePattern(filterValue);
                    if (patternNumbers != null && patternNumbers.Any())
                    {
                        tempBuilder = tempBuilder.Pattern(patternNumbers, numberOfJugglers.Value);
                        return tempBuilder.Build();
                    }
                }
                break;

            case "state":
                var stateObj = ParseState(filterValue);
                if (stateObj != null)
                {
                    tempBuilder = tempBuilder.WithState(stateObj);
                    return tempBuilder.Build();
                }
                break;

            case "flexiblepattern":
            case "flexible_pattern":
                if (numberOfJugglers.HasValue)
                {
                    var groups = ParseFlexiblePattern(filterValue);
                    if (groups != null && groups.Any())
                    {
                        tempBuilder = tempBuilder.FlexiblePattern(
                            groups,
                            numberOfJugglers.Value,
                            isGlobalPattern: true
                        );
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

    /// <summary>
    /// Erstellt einen Filter aus einem Occurrence-String mit OR-Logik-Unterstützung
    /// </summary>
    public ISiteswapFilter? BuildOccurrenceFilterWithOr(
        string occurrenceString,
        Func<IFilterBuilder, IEnumerable<int>, int, IFilterBuilder> addFilter
    )
    {
        if (string.IsNullOrWhiteSpace(occurrenceString))
            return null;

        // Prüfe auf OR-Logik (|)
        if (occurrenceString.Contains('|'))
        {
            var orFilters = new List<ISiteswapFilter>();
            foreach (var orPart in occurrenceString.Split('|'))
            {
                IFilterBuilder tempBuilder = new FilterBuilder(input);
                var parsed = ParseOccurrenceFilters(orPart.Trim());
                foreach (var (numbers, amount) in parsed)
                {
                    tempBuilder = addFilter(tempBuilder, numbers, amount);
                }
                var builtFilter = tempBuilder.Build();
                orFilters.Add(builtFilter);
            }
            if (orFilters.Count > 0)
            {
                var orBuilder = new FilterBuilder(input);
                return orBuilder.Or(orFilters).Build();
            }
        }
        else
        {
            IFilterBuilder tempBuilder = new FilterBuilder(input);
            var parsed = ParseOccurrenceFilters(occurrenceString);
            foreach (var (numbers, amount) in parsed)
            {
                tempBuilder = addFilter(tempBuilder, numbers, amount);
            }
            return tempBuilder.Build();
        }

        return null;
    }

    /// <summary>
    /// Erstellt einen Filter aus einem Pattern-String mit OR-Logik-Unterstützung
    /// </summary>
    public ISiteswapFilter? BuildPatternFilterWithOr(string patternString)
    {
        if (string.IsNullOrWhiteSpace(patternString) || !numberOfJugglers.HasValue)
            return null;

        // Prüfe auf OR-Logik (|)
        if (patternString.Contains('|'))
        {
            var orFilters = new List<ISiteswapFilter>();
            foreach (var orPart in patternString.Split('|'))
            {
                var patternNumbers = ParsePattern(orPart.Trim());
                if (patternNumbers != null && patternNumbers.Any())
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
                return orBuilder.Or(orFilters).Build();
            }
        }
        else
        {
            var patternNumbers = ParsePattern(patternString);
            if (patternNumbers != null && patternNumbers.Any())
            {
                IFilterBuilder tempBuilder = new FilterBuilder(input);
                tempBuilder = tempBuilder.Pattern(patternNumbers, numberOfJugglers.Value);
                return tempBuilder.Build();
            }
        }

        return null;
    }

    /// <summary>
    /// Erstellt einen Filter aus einem State-String mit OR-Logik-Unterstützung
    /// </summary>
    public ISiteswapFilter? BuildStateFilterWithOr(string stateString)
    {
        if (string.IsNullOrWhiteSpace(stateString))
            return null;

        // Prüfe auf OR-Logik (|)
        if (stateString.Contains('|'))
        {
            var orFilters = new List<ISiteswapFilter>();
            foreach (var orPart in stateString.Split('|'))
            {
                var stateObj = ParseState(orPart.Trim());
                if (stateObj != null)
                {
                    IFilterBuilder tempBuilder = new FilterBuilder(input);
                    tempBuilder = tempBuilder.WithState(stateObj);
                    var builtFilter = tempBuilder.Build();
                    orFilters.Add(builtFilter);
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
            var stateObj = ParseState(stateString);
            if (stateObj != null)
            {
                IFilterBuilder tempBuilder = new FilterBuilder(input);
                tempBuilder = tempBuilder.WithState(stateObj);
                return tempBuilder.Build();
            }
        }

        return null;
    }

    /// <summary>
    /// Erstellt einen Filter aus einem FlexiblePattern-String mit OR-Logik-Unterstützung
    /// </summary>
    public ISiteswapFilter? BuildFlexiblePatternFilterWithOr(string flexiblePatternString)
    {
        if (string.IsNullOrWhiteSpace(flexiblePatternString) || !numberOfJugglers.HasValue)
            return null;

        // Prüfe auf OR-Logik (|)
        if (flexiblePatternString.Contains('|'))
        {
            var orFilters = new List<ISiteswapFilter>();
            foreach (var orPart in flexiblePatternString.Split('|'))
            {
                var groups = ParseFlexiblePattern(orPart.Trim());
                if (groups != null && groups.Any())
                {
                    IFilterBuilder tempBuilder = new FilterBuilder(input);
                    tempBuilder = tempBuilder.FlexiblePattern(
                        groups,
                        numberOfJugglers.Value,
                        isGlobalPattern: true
                    );
                    var builtFilter = tempBuilder.Build();
                    orFilters.Add(builtFilter);
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
            var groups = ParseFlexiblePattern(flexiblePatternString);
            if (groups != null && groups.Any())
            {
                IFilterBuilder tempBuilder = new FilterBuilder(input);
                tempBuilder = tempBuilder.FlexiblePattern(
                    groups,
                    numberOfJugglers.Value,
                    isGlobalPattern: true
                );
                return tempBuilder.Build();
            }
        }

        return null;
    }

    /// <summary>
    /// Baut einen kompletten Filter aus allen Filter-Parametern
    /// </summary>
    public IFilterBuilder BuildFilterFromParameters(
        string? minOccurrence = null,
        string? maxOccurrence = null,
        string? exactOccurrence = null,
        int? numberOfPasses = null,
        string? pattern = null,
        string? state = null,
        string? flexiblePattern = null,
        bool useDefaultFilter = true,
        bool useNoFilter = false,
        int? jugglerIndex = null,
        string? rotationAwarePattern = null,
        string? personalizedNumberFilter = null,
        string? notFilter = null
    )
    {
        IFilterBuilder filterBuilder = new FilterBuilder(input);

        // MinimumOccurrence Filter (unterstützt mehrere mit Komma, OR mit |)
        if (!string.IsNullOrWhiteSpace(minOccurrence))
        {
            var minFilter = BuildOccurrenceFilterWithOr(
                minOccurrence,
                (builder, numbers, amount) => builder.MinimumOccurence(numbers, amount)
            );

            if (minFilter != null)
            {
                filterBuilder = filterBuilder.And([minFilter]);
            }
            else
            {
                // Fallback für einfache Fälle ohne OR
                var parsed = ParseOccurrenceFilters(minOccurrence);
                foreach (var (numbers, amount) in parsed)
                {
                    filterBuilder = filterBuilder.MinimumOccurence(numbers, amount);
                }
            }
        }

        // MaximumOccurrence Filter (unterstützt mehrere mit Komma, OR mit |)
        if (!string.IsNullOrWhiteSpace(maxOccurrence))
        {
            var maxFilter = BuildOccurrenceFilterWithOr(
                maxOccurrence,
                (builder, numbers, amount) => builder.MaximumOccurence(numbers, amount)
            );

            if (maxFilter != null)
            {
                filterBuilder = filterBuilder.And([maxFilter]);
            }
            else
            {
                // Fallback für einfache Fälle ohne OR
                var parsed = ParseOccurrenceFilters(maxOccurrence);
                foreach (var (numbers, amount) in parsed)
                {
                    filterBuilder = filterBuilder.MaximumOccurence(numbers, amount);
                }
            }
        }

        // ExactOccurrence Filter (unterstützt mehrere mit Komma, OR mit |)
        if (!string.IsNullOrWhiteSpace(exactOccurrence))
        {
            var exactFilter = BuildOccurrenceFilterWithOr(
                exactOccurrence,
                (builder, numbers, amount) => builder.ExactOccurence(numbers, amount)
            );

            if (exactFilter != null)
            {
                filterBuilder = filterBuilder.And([exactFilter]);
            }
            else
            {
                // Fallback für einfache Fälle ohne OR
                var parsed = ParseOccurrenceFilters(exactOccurrence);
                foreach (var (numbers, amount) in parsed)
                {
                    filterBuilder = filterBuilder.ExactOccurence(numbers, amount);
                }
            }
        }

        // NumberOfPasses Filter
        if (numberOfPasses.HasValue && numberOfJugglers.HasValue)
        {
            filterBuilder = filterBuilder.ExactNumberOfPasses(
                numberOfPasses.Value,
                numberOfJugglers.Value
            );
        }

        // Pattern Filter (unterstützt OR mit |)
        if (!string.IsNullOrWhiteSpace(pattern))
        {
            var patternFilter = BuildPatternFilterWithOr(pattern);
            if (patternFilter != null)
            {
                filterBuilder = filterBuilder.And([patternFilter]);
            }
            else
            {
                // Fallback für einfache Fälle ohne OR
                var patternNumbers = ParsePattern(pattern);
                if (patternNumbers != null && patternNumbers.Any() && numberOfJugglers.HasValue)
                {
                    filterBuilder = filterBuilder.Pattern(patternNumbers, numberOfJugglers.Value);
                }
            }
        }

        // State Filter (unterstützt OR mit |)
        if (!string.IsNullOrWhiteSpace(state))
        {
            var stateFilter = BuildStateFilterWithOr(state);
            if (stateFilter != null)
            {
                filterBuilder = filterBuilder.And([stateFilter]);
            }
            else
            {
                // Fallback für einfache Fälle ohne OR
                var stateObj = ParseState(state);
                if (stateObj != null)
                {
                    filterBuilder = filterBuilder.WithState(stateObj);
                }
            }
        }

        // Flexible Pattern Filter (unterstützt OR mit |)
        if (!string.IsNullOrWhiteSpace(flexiblePattern) && numberOfJugglers.HasValue)
        {
            var flexiblePatternFilter = BuildFlexiblePatternFilterWithOr(flexiblePattern);
            if (flexiblePatternFilter != null)
            {
                filterBuilder = filterBuilder.And([flexiblePatternFilter]);
            }
            else
            {
                // Fallback für einfache Fälle ohne OR
                var groups = ParseFlexiblePattern(flexiblePattern);
                if (groups != null && groups.Any())
                {
                    filterBuilder = filterBuilder.FlexiblePattern(
                        groups,
                        numberOfJugglers.Value,
                        isGlobalPattern: true
                    );
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
            var locallyValidFilter = new LocallyValidFilter(
                numberOfJugglers.Value,
                jugglerIndex.Value
            );
            filterBuilder = filterBuilder.And([locallyValidFilter]);
        }

        // RotationAwareFlexiblePatternFilter (für spezifischen Jongleur)
        if (
            !string.IsNullOrWhiteSpace(rotationAwarePattern)
            && numberOfJugglers.HasValue
            && jugglerIndex.HasValue
        )
        {
            var groups = ParseFlexiblePattern(rotationAwarePattern);
            if (groups != null && groups.Any())
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
            var personalizedFilter = ParsePersonalizedNumberFilter(personalizedNumberFilter);
            if (personalizedFilter != null)
            {
                filterBuilder = filterBuilder.And([personalizedFilter]);
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
            var notFilterObj = ParseAndBuildNotFilter(notFilter);
            if (notFilterObj != null)
            {
                filterBuilder = filterBuilder.Not(notFilterObj);
            }
        }

        return filterBuilder;
    }
}
