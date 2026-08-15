using System.Text.Json;
using System.Text.Json.Serialization;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;

namespace Siteswaps.Generator.Components.WizardPage;

/// <summary>
/// Builds a stable localStorage key from the wizard inputs that feed SiteswapGenerator.
/// The key is the canonical JSON serialization of those inputs (namespaced).
/// </summary>
internal static class WizardGenerationCacheKey
{
    private const string Prefix = "pz-wizard-gen:";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    public static string From(WizardState state)
    {
        var dto = new WizardGenerationInputDto
        {
            Jugglers = state.NumberOfJugglers,
            Period = state.Period.Value,
            ClubsMin = state.Clubs.MinNumber,
            ClubsMax = state.Clubs.MaxNumber,
            ShowThrowNames = state.ShowThrowNames,
            Throws = state
                .AllowedThrows.OrderBy(t => t.Height)
                .ThenBy(t => t.Name, StringComparer.Ordinal)
                .Select(t => t.Name)
                .ToArray(),
            FilterTree = SerializeNode(state.FilterTree.Root),
        };

        return Prefix + JsonSerializer.Serialize(dto, JsonOptions);
    }

    private static object? SerializeNode(FilterNode? node) =>
        node switch
        {
            null => null,
            FilterLeaf leaf => new
            {
                kind = "leaf",
                filter = WizardFilterTree.Unwrap(leaf.Filter).Display(),
            },
            AndNode andNode => new
            {
                kind = "and",
                children = andNode.Children.Select(SerializeNode).ToArray(),
            },
            OrNode orNode => new
            {
                kind = "or",
                children = orNode.Children.Select(SerializeNode).ToArray(),
            },
            _ => null,
        };

    private sealed class WizardGenerationInputDto
    {
        public int Jugglers { get; init; }
        public int Period { get; init; }
        public int ClubsMin { get; init; }
        public int ClubsMax { get; init; }
        public bool ShowThrowNames { get; init; }
        public string[] Throws { get; init; } = [];
        public object? FilterTree { get; init; }
    }
}
