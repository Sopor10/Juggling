using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Siteswaps.Mcp.Server.Prompts;

[McpServerPromptType]
public class SiteswapPrompts
{
    [McpServerPrompt]
    [Description("Analyzes and explains a siteswap in detail. Uses the analyze_siteswap tool and relevant resources.")]
    public static IEnumerable<PromptMessage> ExplainSiteswap(
        [Description("The siteswap to explain (e.g., '531', '441', '97531')")] 
        string siteswap)
    {
        yield return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text = $"""
                    Analyze and explain the siteswap "{siteswap}" in detail.
                    
                    Please perform the following steps:
                    1. Use the 'analyze_siteswap' tool to analyze the siteswap
                    2. Explain the results clearly:
                       - How many objects are required?
                       - What is the period?
                       - What orbits exist?
                       - Is it a ground state or excited state pattern?
                    3. Describe what the pattern looks like when juggled
                    4. Provide tips for learning the pattern
                    
                    If the siteswap is invalid, explain why.
                    """
            }
        };
    }

    [McpServerPrompt]
    [Description("Generates siteswaps based on skill level and preferences.")]
    public static IEnumerable<PromptMessage> GenerateSiteswapsForLevel(
        [Description("Number of objects (e.g., 3, 4, 5)")] 
        int numberOfObjects,
        [Description("Skill level: 'beginner', 'intermediate', 'advanced'")] 
        string level,
        [Description("Optional preference: 'ground_state', 'excited_state', 'sync', 'async'")] 
        string? preference = null)
    {
        var levelDescription = level.ToLower() switch
        {
            "beginner" => "short periods (2-3), low throws, ground state patterns",
            "intermediate" => "medium periods (3-5), moderate throw heights, mixed states",
            "advanced" => "longer periods (5-7), high throws, excited state patterns",
            _ => "medium difficulty"
        };

        var preferenceText = preference != null 
            ? $"\nPreferred style: {preference}" 
            : "";

        yield return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text = $"""
                    Generate suitable siteswaps for the following criteria:
                    
                    - Number of objects: {numberOfObjects}
                    - Level: {level} ({levelDescription}){preferenceText}
                    
                    Please perform the following steps:
                    1. Use the 'generate_siteswaps' tool with appropriate parameters:
                       - period: based on level (beginner: 2-3, intermediate: 3-5, advanced: 5-7)
                       - maxHeight: based on level (beginner: {numberOfObjects + 2}, intermediate: {numberOfObjects + 4}, advanced: {numberOfObjects + 6})
                       - numberOfObjects: {numberOfObjects}
                    2. Analyze 3-5 interesting results with 'analyze_siteswap'
                    3. Briefly explain each pattern and provide learning tips
                    4. Order the patterns by recommended learning sequence
                    """
            }
        };
    }

    [McpServerPrompt]
    [Description("Finds and explains transitions between two siteswaps.")]
    public static IEnumerable<PromptMessage> FindTransitions(
        [Description("Source siteswap (e.g., '3')")] 
        string fromSiteswap,
        [Description("Target siteswap (e.g., '531')")] 
        string toSiteswap,
        [Description("Maximum transition length (default: 3)")] 
        int maxLength = 3)
    {
        yield return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text = $"""
                    Find transitions from "{fromSiteswap}" to "{toSiteswap}".
                    
                    Please perform the following steps:
                    1. Use 'calculate_transitions' with:
                       - fromSiteswap: "{fromSiteswap}"
                       - toSiteswap: "{toSiteswap}"
                       - maxLength: {maxLength}
                    2. Explain the found transitions clearly
                    3. Recommend the simplest/most elegant transition
                    4. Describe how to practice the transition
                    
                    If no transitions are found, explain why and suggest alternatives.
                    """
            }
        };
    }

    [McpServerPrompt]
    [Description("Explains a siteswap concept using the knowledge resources.")]
    public static IEnumerable<PromptMessage> LearnConcept(
        [Description("The concept to explain (e.g., 'orbit', 'state', 'transition', 'hijacking', 'multiplex', 'sync')")] 
        string concept)
    {
        var resourceSuggestion = concept.ToLower() switch
        {
            "orbit" or "orbits" => "siteswap:definition:orbit",
            "state" or "states" or "ground state" or "excited state" => "siteswap:definition:juggling-state or siteswap:definition:ground-state",
            "transition" or "transitions" => "siteswap:definition:transition-throw",
            "hijacking" or "hijack" => "siteswap:definition:hijacking and get_passing_zone_hijacking",
            "multiplex" => "siteswap:definition:multiplex",
            "sync" or "synchronous" => "siteswap:definition:synchronous-siteswap",
            "passing" => "siteswap:definition:passing-siteswap and get_passing_patterns_compendium",
            "vanilla" => "siteswap:definition:vanilla-siteswap",
            _ => "siteswap:definition:siteswap (basics)"
        };

        yield return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text = $"""
                    Explain the siteswap concept "{concept}" in detail.
                    
                    Please perform the following steps:
                    1. Fetch relevant resources:
                       - Recommended resource: {resourceSuggestion}
                       - Additionally: Wikipedia or Juggle Wiki articles if needed
                    2. Explain the concept clearly in your own words
                    3. Provide practical examples with concrete siteswaps
                    4. Use 'analyze_siteswap' to illustrate the examples
                    5. Give tips on how to apply the concept in practice
                    """
            }
        };
    }

    [McpServerPrompt]
    [Description("Creates a practice plan for learning a specific siteswap.")]
    public static IEnumerable<PromptMessage> CreatePracticePlan(
        [Description("The target siteswap to learn")] 
        string targetSiteswap,
        [Description("Current skill: Which patterns can the user already do? (comma-separated, e.g., '3,423,441')")] 
        string knownPatterns)
    {
        yield return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text = $"""
                    Create a practice plan to learn "{targetSiteswap}".
                    
                    Known patterns: {knownPatterns}
                    
                    Please perform the following steps:
                    1. Analyze the target pattern with 'analyze_siteswap'
                    2. Briefly analyze the known patterns
                    3. Find transitions from known patterns to the target with 'calculate_transitions'
                    4. Create a structured practice plan:
                       - Preparatory exercises
                       - Building blocks (simpler variations)
                       - Transitions to the target
                       - The target pattern itself
                    5. Provide time recommendations (how long per exercise)
                    """
            }
        };
    }

    [McpServerPrompt]
    [Description("Compares multiple siteswaps and shows similarities/differences.")]
    public static IEnumerable<PromptMessage> CompareSiteswaps(
        [Description("Siteswaps to compare (comma-separated, e.g., '531,441,423')")] 
        string siteswaps)
    {
        yield return new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text = $"""
                    Compare the following siteswaps: {siteswaps}
                    
                    Please perform the following steps:
                    1. Analyze each siteswap with 'analyze_siteswap'
                    2. Create a transition graph with 'generate_transition_graph' for these patterns
                    3. Compare:
                       - Number of objects
                       - Periods
                       - Orbits and their structure
                       - Ground state vs excited state
                       - Difficulty level
                    4. Highlight similarities and differences
                    5. Recommend a learning order
                    """
            }
        };
    }
}
