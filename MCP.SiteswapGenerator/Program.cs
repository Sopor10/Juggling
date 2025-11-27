using Microsoft.Extensions.Logging;

namespace MCP.SiteswapGenerator;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Logging zu stderr konfigurieren
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Starting MCP Siteswap Generator Server...");

        // TODO: MCP Server Setup - Wird in nächstem Schritt implementiert
        // Sobald die richtige API bekannt ist, wird hier der Server konfiguriert
        
        logger.LogInformation("MCP Server placeholder - waiting for proper API implementation");
        
        // Warten, damit der Prozess nicht sofort beendet wird
        await Task.Delay(Timeout.Infinite);
    }
}
