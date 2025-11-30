using Siteswaps.Mcp.Server.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp();

app.Run();
