using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class GetLocalSiteswapTool
{
    [McpServerTool]
    [Description("Converts a global siteswap to local notation for all jugglers in a passing pattern. Returns the local siteswap notation, global notation, average objects per juggler, and validation information for each juggler.")]
    public LocalSiteswapResult GetLocalSiteswap(
        [Description("Global siteswap string (e.g., '531', 'a7242')")] string siteswap,
        [Description("Number of jugglers in the passing pattern")] int numberOfJugglers)
    {
        if (string.IsNullOrWhiteSpace(siteswap))
        {
            throw new ArgumentException("Siteswap string cannot be null or empty.", nameof(siteswap));
        }

        if (numberOfJugglers < 1)
        {
            throw new ArgumentException("Number of jugglers must be at least 1.", nameof(numberOfJugglers));
        }

        if (!SiteswapDetails.TryCreate(siteswap, out var siteswapObj))
        {
            throw new ArgumentException($"Invalid siteswap: {siteswap}", nameof(siteswap));
        }

        var result = new LocalSiteswapResult
        {
            GlobalSiteswap = siteswap,
            NumberOfJugglers = numberOfJugglers,
            Jugglers = new List<JugglerLocalSiteswap>()
        };

        for (int juggler = 0; juggler < numberOfJugglers; juggler++)
        {
            var localSiteswap = siteswapObj.GetLocalSiteswap(juggler, numberOfJugglers);
            
            result.Jugglers.Add(new JugglerLocalSiteswap
            {
                Juggler = juggler,
                LocalNotation = localSiteswap.LocalNotation,
                GlobalNotation = localSiteswap.GlobalNotation,
                AverageObjects = localSiteswap.Average(),
                IsValidAsGlobalSiteswap = localSiteswap.IsValidAsGlobalSiteswap()
            });
        }

        return result;
    }
}

public class LocalSiteswapResult
{
    public string GlobalSiteswap { get; init; } = string.Empty;
    public int NumberOfJugglers { get; init; }
    public List<JugglerLocalSiteswap> Jugglers { get; init; } = new();
}

public class JugglerLocalSiteswap
{
    public int Juggler { get; init; }
    public string LocalNotation { get; init; } = string.Empty;
    public string GlobalNotation { get; init; } = string.Empty;
    public double AverageObjects { get; init; }
    public bool IsValidAsGlobalSiteswap { get; init; }
}

