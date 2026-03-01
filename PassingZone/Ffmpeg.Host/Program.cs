using System.Text.Json;
using System.Text.Json.Serialization;
using Ffmpeg.Host.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

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

// Configure WordPress Options
builder.Services.Configure<WordPressOptions>(builder.Configuration.GetSection("WordPress"));

// Configure RenderJobProcessor Options
builder.Services.Configure<RenderJobProcessorOptions>(
    builder.Configuration.GetSection("RenderJobProcessor")
);

// Register WordPressService
builder.Services.AddScoped<WordPressService>();

// Register Background Service
builder.Services.AddHostedService<RenderJobProcessor>();

var app = builder.Build();

app.UsePathBase("/ffmpeg");

app.MapGet("/", () => "Ffmpeg.Host is running.");

app.MapPost(
    "/postrender",
    async (
        Input input,
        ILogger<Program> logger,
        IConfiguration configuration,
        CancellationToken cancellationToken
    ) =>
    {
        try
        {
            logger.LogInformation("Received post-render request");

            if (string.IsNullOrEmpty(input.PostId))
            {
                logger.LogError("post_id is required for WordPress upload");
                return TypedResults.Problem("post_id is required");
            }

            // Extract post ID from URI (e.g., https://passing.zone/?post_type=pattern&p=2986)
            if (!TryExtractPostIdFromUri(input.PostId, out var postId))
            {
                logger.LogError(
                    "Invalid post_id format: {PostId}. Expected URI with 'p' query parameter.",
                    input.PostId
                );
                return TypedResults.Problem(
                    "Invalid post_id format. Expected URI with 'p' query parameter."
                );
            }

            logger.LogInformation(
                "Processing post-render request. PostId URI: {PostIdUri}, Extracted PostId: {PostId}",
                input.PostId,
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
                logger.LogInformation("Creating input directory: {InputDirectory}", inputDirectory);
                Directory.CreateDirectory(inputDirectory);
            }

            // Values starting with "{field" are placeholders from the form and treated as empty
            static bool IsEmptyPlaceholder(string? value) =>
                !string.IsNullOrEmpty(value)
                && value.TrimStart().StartsWith("{field", StringComparison.OrdinalIgnoreCase);

            var audioUrl = input.Audio;
            if (audioUrl != null && IsEmptyPlaceholder(audioUrl.ToString()))
                audioUrl = null;

            // Create job object (audio_url is optional; when omitted, original video audio is kept)
            var job = new RenderJob
            {
                PostId = input.PostId,
                Video = input.Video,
                Audio = audioUrl,
                Title = IsEmptyPlaceholder(input.Title) ? null : input.Title,
                Location = IsEmptyPlaceholder(input.Location) ? null : input.Location,
                Jugglers = IsEmptyPlaceholder(input.Jugglers) ? null : input.Jugglers,
                Musicartist = IsEmptyPlaceholder(input.Musicartist) ? null : input.Musicartist,
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
            await File.WriteAllTextAsync(jobFilePath, jsonContent, cancellationToken);

            logger.LogInformation(
                "Job file written successfully. Size: {Size} bytes, PostId: {PostId}",
                jsonContent.Length,
                postId
            );

            return (IResult)
                TypedResults.Ok(
                    new
                    {
                        success = true,
                        postId = postId,
                        message = "Job queued successfully",
                        jobFile = jobFilePath,
                    }
                );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during post-render job creation");
            return (IResult)TypedResults.Problem($"An error occurred: {ex.Message}");
        }
    }
);

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

app.Run();

public record RenderOptions(
    string Title,
    string Location,
    string Jugglers,
    string MusicArtist,
    string? BlockSpacing = null,
    string? InternalSpacing = null
);

public record Input
{
    [JsonPropertyName("post_id")]
    public string? PostId { get; init; }

    [JsonPropertyName("video_url")]
    public Uri? Video { get; init; }

    /// <summary>Optional. When omitted, the original video audio is kept in the rendered output.</summary>
    [JsonPropertyName("audio_url")]
    public Uri? Audio { get; init; }
    public string? Location { get; init; }
    public string? Title { get; init; }
    public string? Jugglers { get; init; }
    public string? Musicartist { get; init; }
}
