var builder = WebApplication.CreateBuilder(args);

// Session-Support hinzufügen für MCP HTTP-Transport
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder
    .Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly();

var app = builder.Build();

// Session-Middleware aktivieren
app.UseSession();

// Middleware: GET-Requests ignorieren (keine Fehlerme ldung)
app.Use(
    async (context, next) =>
    {
        if (context.Request.Method == "GET" && context.Request.Path == "/")
        {
            // 200 OK mit leerer Antwort - damit keine Fehlermeldung in der UI erscheint
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("");
            return;
        }
        await next();
    }
);

app.MapMcp();

app.Run();
