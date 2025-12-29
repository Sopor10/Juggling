using System.ComponentModel;
using ModelContextProtocol.Server;
using Siteswap.Details;
using SiteswapDetails = Siteswap.Details.Siteswap;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class GetLocalSiteswapTool
{
    [McpServerTool]
    [Description(
        "Converts a global siteswap to local notation for a specific juggler in a passing pattern. Returns the local siteswap notation, global notation, average objects per juggler, and validation information."
    )]
    public ToolResult<LocalSiteswapInfo> GetLocalSiteswap(
        [Description("Global siteswap string (e.g., '5,3,1', 'a,7,2,4,2')")] string siteswap,
        [Description("Juggler index (0-based, e.g., 0 for first juggler, 1 for second juggler)")]
            int juggler,
        [Description("Number of jugglers in the passing pattern")] int numberOfJugglers
    )
    {
        return ToolResult.From(() =>
        {
            var coreSiteswap = SiteswapMapper.ToCoreFormat(siteswap);
            if (string.IsNullOrWhiteSpace(coreSiteswap))
            {
                throw new ArgumentException(
                    "Siteswap string cannot be null or empty.",
                    nameof(siteswap)
                );
            }

            if (juggler < 0)
            {
                throw new ArgumentException("Juggler index must be non-negative.", nameof(juggler));
            }

            if (numberOfJugglers < 1)
            {
                throw new ArgumentException(
                    "Number of jugglers must be at least 1.",
                    nameof(numberOfJugglers)
                );
            }

            if (juggler >= numberOfJugglers)
            {
                throw new ArgumentException(
                    $"Juggler index ({juggler}) must be less than number of jugglers ({numberOfJugglers}).",
                    nameof(juggler)
                );
            }

            if (!SiteswapDetails.TryCreate(coreSiteswap, out var siteswapObj))
            {
                throw new ArgumentException($"Invalid siteswap: {siteswap}", nameof(siteswap));
            }

            var localSiteswap = siteswapObj.GetLocalSiteswap(juggler, numberOfJugglers);

            var clubs = siteswapObj.GetClubDistribution(numberOfJugglers);

            return new LocalSiteswapInfo
            {
                GlobalSiteswap = SiteswapMapper.ToDisplayFormat(siteswapObj),
                Juggler = juggler,
                NumberOfJugglers = numberOfJugglers,
                LocalNotation = SiteswapMapper.LocalToDisplayFormat(localSiteswap.LocalNotation),
                GlobalNotation = SiteswapMapper.ToDisplayFormat(localSiteswap.GlobalNotation),
                AverageObjectsPerJuggler = localSiteswap.Average(),
                IsValidAsGlobalSiteswap = localSiteswap.IsValidAsGlobalSiteswap(),
                ClubDistribution = string.Join(
                    "|",
                    clubs.Hands.Where(x => x.Item1.Juggler == juggler).Select(x => x.Item2)
                ),
            };
        });
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
    public string ClubDistribution { get; init; } = string.Empty;
}
