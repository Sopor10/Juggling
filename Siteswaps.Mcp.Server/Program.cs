using Siteswaps.Mcp.Server.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<GenerateSiteswapsTool>()
    .WithTools<ValidateSiteswapTool>()
    .WithTools<AnalyzeSiteswapTool>()
    .WithTools<CalculateTransitionsTool>();

var app = builder.Build();

app.MapMcp();

app.Run();