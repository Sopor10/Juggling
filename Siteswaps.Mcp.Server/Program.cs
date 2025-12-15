using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly();

var app = builder.Build();

// Middleware zum Loggen der URI für resources/read Requests
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" && context.Request.Method == "POST")
    {
        // Request-Body lesen und zurückspulen
        context.Request.EnableBuffering();
        var originalBodyStream = context.Request.Body;
        
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        
        if (!string.IsNullOrEmpty(body))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("method", out var methodElement))
                {
                    var method = methodElement.GetString();
                    if (method == "resources/read")
                    {
                        if (doc.RootElement.TryGetProperty("params", out var paramsElement))
                        {
                            if (paramsElement.TryGetProperty("uri", out var uriElement))
                            {
                                var uri = uriElement.GetString();
                                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                                logger.LogInformation("resources/read request handler called with URI: {Uri}", uri);
                            }
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore JSON parsing errors
            }
        }
    }
    
    await next();
});

app.MapMcp();

app.Run();