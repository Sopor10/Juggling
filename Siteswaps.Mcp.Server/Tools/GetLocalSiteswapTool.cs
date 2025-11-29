using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

public class GetLocalSiteswapTool
{
    [McpServerTool]
    [Description("Converts a global siteswap to local notation for a specific juggler in a passing pattern. Returns the local siteswap notation, global notation, average objects per juggler, and validation information.")]
    public LocalSiteswapInfo GetLocalSiteswap(
        [Description("Global siteswap string (e.g., '531', 'a7242')")] string siteswap,
        [Description("Juggler index (0-based, e.g., 0 for first juggler, 1 for second juggler)")] int juggler,
        [Description("Number of jugglers in the passing pattern")] int numberOfJugglers)
    {
        if (string.IsNullOrWhiteSpace(siteswap))
        {
            throw new ArgumentException("Siteswap string cannot be null or empty.", nameof(siteswap));
        }

        if (juggler < 0)
        {
            throw new ArgumentException("Juggler index must be non-negative.", nameof(juggler));
        }

        if (numberOfJugglers < 1)
        {
            throw new ArgumentException("Number of jugglers must be at least 1.", nameof(numberOfJugglers));
        }

        if (juggler >= numberOfJugglers)
        {
            throw new ArgumentException($"Juggler index ({juggler}) must be less than number of jugglers ({numberOfJugglers}).", nameof(juggler));
        }

        if (!SiteswapDetails.TryCreate(siteswap, out var siteswapObj))
        {
            throw new ArgumentException($"Invalid siteswap: {siteswap}", nameof(siteswap));
        }

        var localSiteswap = siteswapObj.GetLocalSiteswap(juggler, numberOfJugglers);

        return new LocalSiteswapInfo
        {
            GlobalSiteswap = siteswap,
            Juggler = juggler,
            NumberOfJugglers = numberOfJugglers,
            LocalNotation = localSiteswap.LocalNotation,
            GlobalNotation = localSiteswap.GlobalNotation,
            AverageObjectsPerJuggler = localSiteswap.Average(),
            IsValidAsGlobalSiteswap = localSiteswap.IsValidAsGlobalSiteswap()
        };
    }
}

public class LocalSiteswapInfo
{
    public string GlobalSiteswap { get; init; } = string.Empty;
    public int Juggler { get; init; }
    public int NumberOfJugglers { get; init; }
    public string LocalNotation { get; init; } = string.Empty;
    public string GlobalNotation { get; init; } = string.Empty;
    public double AverageObjectsPerJuggler { get; init; }
    public bool IsValidAsGlobalSiteswap { get; init; }
}

