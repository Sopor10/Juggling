using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Siteswaps.Mcp.Server.Prompts;

[McpServerPromptType]
public class SiteswapPrompts
{
    [McpServerPrompt]
    [Description(
        "Analyzes and explains a siteswap in detail. Uses the analyze_siteswap tool and relevant resources."
    )]
    public static IEnumerable<PromptMessage> ExplainSiteswap(
        [Description("The siteswap to explain (e.g., '531', '441', '97531')")] string siteswap
    )
    {
        yield return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text = $"Analyze and explain the siteswap \"{siteswap}\" in detail.",
            },
        };
    }

    [McpServerPrompt]
    [Description("Generates siteswaps based on skill level and preferences.")]
    public static IEnumerable<PromptMessage> GenerateSiteswapsForLevel(
        [Description("Number of objects (e.g., 3, 4, 5)")] int numberOfObjects,
        [Description("Skill level: 'beginner', 'intermediate', 'advanced'")] string level,
        [Description("Optional preference")] string? preference = null
    )
    {
        var levelDescription = level.ToLowerInvariant() switch
        {
            "beginner" => "short periods (2-3), low throws, ground state patterns",
            "intermediate" => "medium periods (3-5), moderate throw heights, mixed states",
            "advanced" => "longer periods (5-7), high throws, excited state patterns",
            _ => "medium difficulty",
        };
        yield return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text =
                    $"Generate suitable siteswaps for {numberOfObjects} objects at {level} ({levelDescription}). Preference: {preference}",
            },
        };
    }

    [McpServerPrompt]
    [Description("Finds and explains transitions between two siteswaps.")]
    public static IEnumerable<PromptMessage> FindTransitions(
        string fromSiteswap,
        string toSiteswap,
        int maxLength = 3
    )
    {
        yield return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text =
                    $"Find transitions from \"{fromSiteswap}\" to \"{toSiteswap}\" with maximum length {maxLength}.",
            },
        };
    }

    [McpServerPrompt]
    [Description("Explains a siteswap concept using the knowledge resources.")]
    public static IEnumerable<PromptMessage> LearnConcept(string concept)
    {
        var resourceSuggestion = concept.ToLowerInvariant() switch
        {
            "orbit" or "orbits" => "siteswap:definition:orbit",
            "state" or "states" => "siteswap:definition:juggling-state",
            "transition" or "transitions" => "siteswap:definition:transition-throw",
            "hijacking" or "hijack" => "siteswap:definition:hijacking",
            "multiplex" => "siteswap:definition:multiplex",
            "sync" or "synchronous" => "siteswap:definition:synchronous-siteswap",
            _ => "siteswap:definition:siteswap (basics)",
        };
        yield return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text =
                    $"Explain the siteswap concept \"{concept}\". Recommended resource: {resourceSuggestion}.",
            },
        };
    }

    [McpServerPrompt]
    [Description("Creates a practice plan for learning a specific siteswap.")]
    public static IEnumerable<PromptMessage> CreatePracticePlan(
        string targetSiteswap,
        string knownPatterns
    )
    {
        yield return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text =
                    $"Create a practice plan to learn \"{targetSiteswap}\". Known patterns: {knownPatterns}.",
            },
        };
    }

    [McpServerPrompt]
    [Description("Compares multiple siteswaps and shows similarities/differences.")]
    public static IEnumerable<PromptMessage> CompareSiteswaps(string siteswaps)
    {
        yield return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text = $"Compare the following siteswaps: {siteswaps}.",
            },
        };
    }
}
