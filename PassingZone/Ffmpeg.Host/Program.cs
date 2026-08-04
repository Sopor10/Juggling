using System.Text;
using System.Text.Json;
using Ffmpeg.Host.Services;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.secrets.json", optional: false);

var configuredInputDirectory = builder.Configuration.GetValue<string>(
    "RenderJobProcessor:InputDirectory"
);
var inputDirectory = string.IsNullOrEmpty(configuredInputDirectory)
    ? Path.Combine(Path.GetTempPath(), "ffmpeg-jobs")
    : configuredInputDirectory;
var logDirectory = Path.Combine(inputDirectory, "logs");
Directory.CreateDirectory(logDirectory);

builder.Host.UseSerilog(
    (_, configuration) =>
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Logger(lc =>
                lc.Filter.ByIncludingOnly(e => e.Properties.ContainsKey("PostId"))
                    .WriteTo.Map(
                        "PostId",
                        (postId, wt) =>
                        {
                            var key = postId?.ToString()?.Trim('"') ?? "unknown";
                            wt.File(
                                Path.Combine(logDirectory, $"{key}.log"),
                                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level:u3} | {Message:lj}{NewLine}{Exception}",
                                shared: true
                            );
                        },
                        sinkMapCountLimit: 10
                    )
            )
);

// Increase request size and timeout limits for large video uploads
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 200 * 1024 * 1024; // 200 MB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 200 * 1024 * 1024; // 200 MB
});

builder.Services.AddHttpClient();
builder.Services.AddHealthChecks();

// Configure WordPress Options
builder.Services.Configure<WordPressOptions>(builder.Configuration.GetSection("WordPress"));

// Configure RenderJobProcessor Options
builder.Services.Configure<RenderJobProcessorOptions>(
    builder.Configuration.GetSection("RenderJobProcessor")
);

// Register WordPressService
builder.Services.AddScoped<WordPressService>();

builder.Services.AddSingleton<JobStatusStore>();

// Register Background Service
builder.Services.AddHostedService<RenderJobProcessor>();

var app = builder.Build();

app.UsePathBase("/ffmpeg");
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/", () => "Ffmpeg.Host is running.");

app.MapHealthChecks("/health");

app.MapGet(
    "/api/jobs",
    (JobStatusStore store) =>
        Results.Ok(
            store
                .GetAll()
                .Select(j => new
                {
                    j.PostId,
                    j.Title,
                    Status = j.Status.ToString(),
                    j.QueuedAt,
                    j.StartedAt,
                    j.UpdatedAt,
                    j.FinishedAt,
                    j.LastError,
                    j.MediaId,
                    j.VideoUrl,
                    j.VideoSizeBytes,
                    j.PatternUrl,
                })
        )
);

app.MapGet(
    "/api/jobs/{postId:int}",
    async (int postId, JobStatusStore store, CancellationToken cancellationToken) =>
    {
        var job = store.Get(postId);
        if (job is null)
            return Results.NotFound(new { message = $"Job {postId} not found" });

        var logs = await store.ReadLogsAsync(postId, cancellationToken);
        return Results.Ok(
            new
            {
                job.PostId,
                job.Title,
                Status = job.Status.ToString(),
                job.QueuedAt,
                job.StartedAt,
                job.UpdatedAt,
                job.FinishedAt,
                job.LastError,
                job.MediaId,
                job.VideoUrl,
                job.VideoSizeBytes,
                job.PatternUrl,
                logs,
            }
        );
    }
);

app.MapGet(
    "/jobs",
    async (IWebHostEnvironment env) =>
    {
        var path = Path.Combine(
            env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"),
            "jobs.html"
        );
        if (!File.Exists(path))
            return Results.NotFound("jobs.html not found");
        return Results.File(path, "text/html");
    }
);

app.MapPost(
    "/postrender",
    async (
        HttpRequest request,
        ILogger<Program> logger,
        IConfiguration configuration,
        JobStatusStore jobStatusStore,
        CancellationToken cancellationToken
    ) =>
    {
        string? rawBody = null;
        try
        {
            using (var reader = new StreamReader(request.Body, Encoding.UTF8))
            {
                rawBody = await reader.ReadToEndAsync(cancellationToken);
            }

            logger.LogInformation("Received post-render webhook body: {WebhookBody}", rawBody);

            var input = JsonSerializer.Deserialize<Input>(
                rawBody,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
            );

            if (input is null)
            {
                logger.LogError("Webhook body could not be deserialized to Input");
                return TypedResults.Problem("Invalid JSON body");
            }

            var wordPressBaseUrl =
                configuration.GetValue<string>("WordPress:BaseUrl") ?? "https://passing.zone/";

            if (
                !TryResolvePostId(input.PostId, wordPressBaseUrl, out var postId, out var postIdUri)
            )
            {
                logger.LogError(
                    "post_id is required for WordPress upload. Got: {PostId}",
                    input.PostId
                );
                return TypedResults.Problem(
                    "post_id is required (numeric ID or WordPress URI with p= query parameter)"
                );
            }

            using (logger.BeginScope(new Dictionary<string, object> { ["PostId"] = postId }))
            {
                logger.LogInformation(
                    "Processing post-render request. PostId URI: {PostIdUri}, Extracted PostId: {PostId}",
                    postIdUri,
                    postId
                );

                // Get input directory from configuration (same logic as RenderJobProcessor)
                var configuredInputDirectory = configuration.GetValue<string>(
                    "RenderJobProcessor:InputDirectory"
                );
                var inputDirectory = string.IsNullOrEmpty(configuredInputDirectory)
                    ? Path.Combine(Path.GetTempPath(), "ffmpeg-jobs")
                    : configuredInputDirectory;

                logger.LogInformation("Input directory: {InputDirectory}", inputDirectory);

                // Ensure input directory exists
                if (!Directory.Exists(inputDirectory))
                {
                    logger.LogInformation(
                        "Creating input directory: {InputDirectory}",
                        inputDirectory
                    );
                    Directory.CreateDirectory(inputDirectory);
                }

                // Values starting with "{field" are placeholders from the form and treated as empty
                static bool IsEmptyPlaceholder(string? value) =>
                    !string.IsNullOrEmpty(value)
                    && value.TrimStart().StartsWith("{field", StringComparison.OrdinalIgnoreCase);

                static string? CleanText(string? value) =>
                    string.IsNullOrWhiteSpace(value) || IsEmptyPlaceholder(value)
                        ? null
                        : value.Trim();

                static Uri? TryParseUri(string? value)
                {
                    value = CleanText(value);
                    if (value is null)
                        return null;
                    return Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null;
                }

                var videoUrl = TryParseUri(input.ResolveVideoUrl());
                if (videoUrl is null)
                {
                    logger.LogError(
                        "video_url / raw_video_url is required. Got: {VideoUrl}",
                        input.ResolveVideoUrl()
                    );
                    return TypedResults.Problem("video_url / raw_video_url is required");
                }

                // Create job object (audio_url is optional; when omitted, original video audio is kept)
                var job = new RenderJob
                {
                    PostId = postIdUri,
                    Video = videoUrl,
                    Audio = TryParseUri(input.ResolveAudioUrl()),
                    Title = CleanText(input.Title),
                    Location = CleanText(input.Location),
                    Jugglers = CleanText(input.ResolveJugglers()),
                    Musicartist = CleanText(input.ResolveMusicArtist()),
                };

                // Write job file (filename = PostId.json)
                var jobFilePath = Path.Combine(inputDirectory, $"{postId}.json");
                logger.LogInformation(
                    "Writing job file: {JobFilePath} for PostId: {PostId}",
                    jobFilePath,
                    postId
                );

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                var jsonContent = JsonSerializer.Serialize(job, jsonOptions);
                await File.WriteAllTextAsync(
                    jobFilePath,
                    jsonContent,
                    Encoding.UTF8,
                    cancellationToken
                );

                jobStatusStore.Enqueue(postId, job.Title, postIdUri);

                logger.LogInformation(
                    "Job file written successfully. Size: {Size} bytes, PostId: {PostId}",
                    jsonContent.Length,
                    postId
                );

                return TypedResults.Ok(
                    new
                    {
                        success = true,
                        postId = postId,
                        message = "Job queued successfully",
                        jobFile = jobFilePath,
                    }
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred during post-render job creation. WebhookBody: {WebhookBody}",
                rawBody
            );
            return (IResult)TypedResults.Problem($"An error occurred: {ex.Message}");
        }
    }
);

/// <summary>
/// Resolves a Gravity Forms / legacy webhook post_id value.
/// Accepts a numeric ID (e.g. 3554) or a WordPress URI with ?p=.
/// </summary>
bool TryResolvePostId(
    string? postIdValue,
    string wordPressBaseUrl,
    out int postId,
    out string postIdUri
)
{
    postId = 0;
    postIdUri = string.Empty;

    if (string.IsNullOrWhiteSpace(postIdValue))
        return false;

    postIdValue = postIdValue.Trim();

    if (int.TryParse(postIdValue, out postId) && postId > 0)
    {
        var baseUrl = wordPressBaseUrl.TrimEnd('/');
        postIdUri = $"{baseUrl}/?post_type=pattern&p={postId}";
        return true;
    }

    if (!TryExtractPostIdFromUri(postIdValue, out postId))
        return false;

    postIdUri = postIdValue;
    return true;
}

/// <summary>
/// Extracts the post ID from a WordPress URI.
/// Expected format: https://passing.zone/?post_type=pattern&p=2986
/// </summary>
bool TryExtractPostIdFromUri(string postIdUri, out int postId)
{
    postId = 0;

    if (string.IsNullOrWhiteSpace(postIdUri))
        return false;

    // Try to parse as URI
    if (!Uri.TryCreate(postIdUri, UriKind.Absolute, out var uri))
    {
        return false;
    }

    // Extract 'p' query parameter manually
    var query = uri.Query;
    if (string.IsNullOrEmpty(query))
    {
        return false;
    }

    // Remove leading '?'
    if (query.StartsWith('?'))
    {
        query = query.Substring(1);
    }

    // Parse query parameters
    var parameters = query.Split('&');
    foreach (var param in parameters)
    {
        var parts = param.Split('=', 2);
        if (parts.Length == 2 && parts[0].Equals("p", StringComparison.OrdinalIgnoreCase))
        {
            var pValue = Uri.UnescapeDataString(parts[1]);
            if (int.TryParse(pValue, out postId))
            {
                return true;
            }
        }
    }

    return false;
}

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
