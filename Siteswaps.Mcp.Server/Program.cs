using System.Text.Json;
using ModelContextProtocol.Protocol;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddMcpServer(options =>
        options.ServerInfo = new Implementation { Name = "Siteswaps.Mcp.Server", Version = "1.0.0" }
    )
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

var app = builder.Build();

// Middleware zum Loggen der URI für resources/read Requests
app.Use(
    async (context, next) =>
    {
        if (context.Request.Path == "/" && context.Request.Method == "POST")
        {
            // Request-Body lesen und zurückspulen
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrEmpty(body))
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("method", out var methodElement))
                    {
                        var method = methodElement.GetString();
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                        switch (method)
                        {
                            case "resources/list":
                                logger.LogInformation("resources/list request handler called");
                                break;
                            case "resources/read":
                            {
                                if (doc.RootElement.TryGetProperty("params", out var paramsElement))
                                    if (paramsElement.TryGetProperty("uri", out var uriElement))
                                    {
                                        var uri = uriElement.GetString();
                                        logger.LogInformation(
                                            "resources/read request handler called with URI: {Uri}",
                                            uri
                                        );
                                    }

                                break;
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // Ignore JSON parsing errors
                }
        }

        await next();
    }
);

app.MapMcp();

app.Run();
