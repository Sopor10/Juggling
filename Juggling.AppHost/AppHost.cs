using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Juggling.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDotnetProject("Webassembly", "../Webassembly/Webassembly.csproj")
    .WithTailscaleServe(builder);

builder.AddDotnetProject("McpServer", "../Siteswaps.Mcp.Server/Siteswaps.Mcp.Server.csproj");

builder.Build().Run();
