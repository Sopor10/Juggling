using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MCP.SiteswapGenerator.Tools;

// MCP Server mit Dependency Injection erstellen
var builder = Host.CreateApplicationBuilder(args);

// Logging auf stderr umleiten (stdout wird für JSON-RPC benötigt)
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// MCP Server konfigurieren
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<GenerateSiteswapsTool>(); // Tool explizit registrieren

var host = builder.Build();
await host.RunAsync();