var builder = DistributedApplication.CreateBuilder(args);

builder.AddDotnetProject("Webassembly", "../Webassembly/Webassembly.csproj");
builder.AddDotnetProject("McpServer", "../Siteswaps.Mcp.Server/Siteswaps.Mcp.Server.csproj");

builder.Build().Run();
