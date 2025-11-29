using MCP.SiteswapGenerator.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<GenerateSiteswapsTool>()
    .WithTools<ValidateSiteswapTool>();

var app = builder.Build();

app.MapMcp();

app.Run();