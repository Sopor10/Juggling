using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace Siteswaps.Mcp.Server.Tools;

public class ResourceTools
{
    private static readonly string ResourcesPath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? AppDomain.CurrentDomain.BaseDirectory
            ?? Directory.GetCurrentDirectory(),
        "Resources"
    );

    [McpServerTool]
    [Description(
        "Get the Wikipedia article about Siteswap notation - comprehensive explanation of siteswap notation, its history, basics and extensions"
    )]
    public async Task<string> GetWikipediaSiteswapArticle()
    {
        var filePath = Path.Combine(ResourcesPath, "wikipedia-siteswap.txt");
        return await File.ReadAllTextAsync(filePath);
    }

    [McpServerTool]
    [Description(
        "Get the Siteswap FAQ from juggling.org - detailed FAQ by Allen Knutson about Siteswap notation, Vanilla Siteswap, State Diagrams and more"
    )]
    public async Task<string> GetJugglingOrgFaq()
    {
        var filePath = Path.Combine(ResourcesPath, "juggling-org-faq.txt");
        return await File.ReadAllTextAsync(filePath);
    }

    [McpServerTool]
    [Description(
        "Get the Juggle Wiki Siteswap article - comprehensive article about Siteswap notation with examples, properties and extensions"
    )]
    public async Task<string> GetJuggleFandomSiteswap()
    {
        var filePath = Path.Combine(ResourcesPath, "juggle-fandom-siteswap.txt");
        return await File.ReadAllTextAsync(filePath);
    }

    [McpServerTool]
    [Description(
        "Get article about Hijacking in Period 3, 5 and 7 - article about hijacking techniques in passing, transitions between patterns and general rules"
    )]
    public async Task<string> GetPassingZoneHijacking()
    {
        var filePath = Path.Combine(ResourcesPath, "passing-zone-hijacking.txt");
        return await File.ReadAllTextAsync(filePath);
    }
}
