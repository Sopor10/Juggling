using MCP.SiteswapGenerator.Tools;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<GenerateSiteswapsTool>();

var app = builder.Build();

app.MapMcp();

app.Run();