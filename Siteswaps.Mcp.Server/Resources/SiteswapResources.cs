using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace Siteswaps.Mcp.Server.Resources;

[McpServerResourceType]
public class SiteswapResources
{
    private static readonly string ResourcesPath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? 
        AppDomain.CurrentDomain.BaseDirectory ?? 
        Directory.GetCurrentDirectory(),
        "Resources");

    [McpServerResource]
    [Description("Wikipedia Artikel über Siteswap Notation - Umfassende Erklärung der Siteswap-Notation, ihrer Geschichte, Grundlagen und Erweiterungen")]
    public async Task<string> GetWikipediaSiteswap()
    {
        var filePath = Path.Combine(ResourcesPath, "wikipedia-siteswap.txt");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Resource file not found: {filePath}");
        }
        
        return await File.ReadAllTextAsync(filePath);
    }

    [McpServerResource]
    [Description("Siteswap FAQ von juggling.org - Detaillierte FAQ von Allen Knutson über Siteswap-Notation, Vanilla Siteswap, State Diagrams und mehr")]
    public async Task<string> GetJugglingOrgFaq()
    {
        var filePath = Path.Combine(ResourcesPath, "juggling-org-faq.txt");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Resource file not found: {filePath}");
        }
        
        return await File.ReadAllTextAsync(filePath);
    }

    [McpServerResource]
    [Description("Juggle Wiki Siteswap Artikel - Umfassender Artikel über Siteswap-Notation mit Beispielen, Eigenschaften und Erweiterungen")]
    public async Task<string> GetJuggleFandomSiteswap()
    {
        var filePath = Path.Combine(ResourcesPath, "juggle-fandom-siteswap.txt");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Resource file not found: {filePath}");
        }
        
        return await File.ReadAllTextAsync(filePath);
    }

    [McpServerResource]
    [Description("Hijacking in Period 3, 5 and 7 - Artikel über Hijacking-Techniken beim Passing, Übergänge zwischen Patterns und allgemeine Regeln")]
    public async Task<string> GetPassingZoneHijacking()
    {
        var filePath = Path.Combine(ResourcesPath, "passing-zone-hijacking.txt");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Resource file not found: {filePath}");
        }
        
        return await File.ReadAllTextAsync(filePath);
    }

    [McpServerResource]
    [Description("Highgate Collection PDF - Aidan Burns' Sammlung von Passing-Patterns mit Hijacking-Techniken")]
    public async Task<byte[]> GetHighgateCollection()
    {
        var filePath = Path.Combine(ResourcesPath, "highgate.pdf");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Resource file not found: {filePath}");
        }
        
        return await File.ReadAllBytesAsync(filePath);
    }

    [McpServerResource]
    [Description("Passing Patterns Compendium PDF - Umfassende Sammlung von Passing-Patterns")]
    public async Task<byte[]> GetPassingPatternsCompendium()
    {
        var filePath = Path.Combine(ResourcesPath, "passingpatternscompendium.pdf");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Resource file not found: {filePath}");
        }
        
        return await File.ReadAllBytesAsync(filePath);
    }

    [McpServerResource]
    [Description("Takeouts PDF - Dokumentation über Takeout-Techniken beim Passing")]
    public async Task<byte[]> GetTakeouts()
    {
        var filePath = Path.Combine(ResourcesPath, "takeouts.pdf");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Resource file not found: {filePath}");
        }
        
        return await File.ReadAllBytesAsync(filePath);
    }
}

