using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

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

        try
        {
            // MCP Server mit Stdio Transport erstellen
            var options = new McpServerOptions();
            ITransport transport = new StdioServerTransport(options, loggerFactory);
            
            // Tools werden automatisch durch [McpServerTool] Attribute erkannt
            var server = McpServer.Create(transport, options);

            logger.LogInformation("MCP Server started successfully");
            await server.RunAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running MCP server");
            Environment.Exit(1);
        }
    }
}
