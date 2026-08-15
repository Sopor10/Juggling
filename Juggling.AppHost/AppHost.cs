var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Webassembly>("Webassembly");
builder.AddProject<Projects.Siteswaps_Mcp_Server>("McpServer");

builder.Build().Run();
