using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Ffmpeg.Host.Services;

public class RenderJobProcessor(
    IOptions<RenderJobProcessorOptions> options,
    ILogger<RenderJobProcessor> logger,
    IServiceProvider serviceProvider
) : BackgroundService
{
    private readonly RenderJobProcessorOptions _options = options.Value;
    private readonly ConcurrentDictionary<string, bool> _processingFiles = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Use fallback if InputDirectory is not configured
        var inputDirectory = string.IsNullOrEmpty(_options.InputDirectory)
            ? Path.Combine(Path.GetTempPath(), "ffmpeg-jobs")
            : _options.InputDirectory;

        logger.LogInformation(
            "RenderJobProcessor started. Input directory: {InputDirectory}, Scan interval: {Interval} seconds",
            inputDirectory,
            _options.ScanIntervalSeconds
        );

        // Ensure input directory exists
        if (!Directory.Exists(inputDirectory))
        {
            logger.LogInformation("Creating input directory: {InputDirectory}", inputDirectory);
            Directory.CreateDirectory(inputDirectory);
        }

        // Update options with resolved directory
        _options.InputDirectory = inputDirectory;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in RenderJobProcessor main loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.ScanIntervalSeconds), stoppingToken);
        }

        logger.LogInformation("RenderJobProcessor stopped");
    }

    private async Task ProcessJobsAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Scanning input directory: {InputDirectory}", _options.InputDirectory);

        var jsonFiles = Directory.GetFiles(_options.InputDirectory, "*.json").ToList();

        logger.LogInformation("Found {Count} job file(s) in input directory", jsonFiles.Count);

        foreach (var jobFile in jsonFiles)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            // Check if file is already being processed
            var normalizedPath = Path.GetFullPath(jobFile);
            if (_processingFiles.ContainsKey(normalizedPath))
            {
                logger.LogDebug("Skipping job file {JobFile} - already being processed", jobFile);
                continue;
            }

            try
            {
                // Mark file as being processed
                if (!_processingFiles.TryAdd(normalizedPath, true))
                {
                    logger.LogWarning("Failed to mark job file as processing: {JobFile}", jobFile);
                    continue;
                }

                logger.LogInformation(
                    "Processing job file: {JobFile} (currently processing {Count} file(s))",
                    jobFile,
                    _processingFiles.Count
                );
                await ProcessJobFileAsync(jobFile, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing job file {JobFile}", jobFile);
                // Continue with next file even if one fails
            }
            finally
            {
                // Remove file from processing list
                _processingFiles.TryRemove(normalizedPath, out _);
                logger.LogDebug(
                    "Removed job file from processing list: {JobFile} (currently processing {Count} file(s))",
                    jobFile,
                    _processingFiles.Count
                );
            }
        }
    }

    private async Task ProcessJobFileAsync(string jobFilePath, CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(jobFilePath);
        logger.LogInformation("Reading job file: {FileName}", fileName);

        string jsonContent;
        try
        {
            jsonContent = await File.ReadAllTextAsync(
                jobFilePath,
                Encoding.UTF8,
                cancellationToken
            );
            logger.LogDebug("Successfully read job file. Size: {Size} bytes", jsonContent.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read job file: {JobFile}", jobFilePath);
            throw;
        }

        RenderJob? job;
        try
        {
            job = JsonSerializer.Deserialize<RenderJob>(
                jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            logger.LogDebug("Successfully deserialized job file. PostId: {PostId}", job?.PostId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize job file: {JobFile}", jobFilePath);
            throw;
        }

        if (job == null)
        {
            logger.LogError("Job file deserialized to null: {JobFile}", jobFilePath);
            throw new InvalidOperationException("Job is null after deserialization");
        }

        if (string.IsNullOrEmpty(job.PostId))
        {
            logger.LogError("Job file missing post_id: {JobFile}", jobFilePath);
            throw new InvalidOperationException("Job missing post_id");
        }

        // Extract post ID and post type from URI (e.g., https://passing.zone/?post_type=pattern&p=2986)
        if (!TryExtractPostIdFromUri(job.PostId, out var postId, out var postType))
        {
            logger.LogError(
                "Invalid post_id format in job file: {JobFile}, PostId: {PostId}. Expected URI with 'p' query parameter.",
                jobFilePath,
                job.PostId
            );
            throw new InvalidOperationException(
                $"Invalid post_id format: {job.PostId}. Expected URI with 'p' query parameter."
            );
        }

        logger.LogInformation(
            "Starting render and upload process. PostId URI: {PostIdUri}, Extracted PostId: {PostId}, PostType: {PostType}",
            job.PostId,
            postId,
            postType
        );

        // Create a scope for scoped services
        using var scope = serviceProvider.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var wordPressService = scope.ServiceProvider.GetRequiredService<WordPressService>();
        var logger1 = scope.ServiceProvider.GetRequiredService<ILogger<Renderer>>();

        var httpClient = httpClientFactory.CreateClient();

        var workingDir = Path.Combine(
            Path.GetTempPath(),
            "ffmpeg-render",
            Guid.NewGuid().ToString()
        );

        logger.LogInformation(
            "Created working directory: {WorkingDir} for PostId: {PostId}",
            workingDir,
            postId
        );

        Directory.CreateDirectory(workingDir);

        try
        {
            // Download video if provided
            if (job.Video != null)
            {
                logger.LogInformation(
                    "Downloading video from {Url} for PostId: {PostId}",
                    job.Video,
                    postId
                );
                var videoPath = Path.Combine(workingDir, "video.mp4");
                await using var stream = await httpClient.GetStreamAsync(
                    job.Video,
                    cancellationToken
                );
                await using var fileStream = File.Create(videoPath);
                await stream.CopyToAsync(fileStream, cancellationToken);
                logger.LogInformation(
                    "Video downloaded successfully. Path: {Path}, PostId: {PostId}",
                    videoPath,
                    postId
                );
            }

            // Download audio if provided (ignore placeholders like "{field:...}" from forms)
            var audioUrl = job.Audio;
            if (audioUrl != null)
            {
                var urlString = audioUrl.ToString();
                if (
                    string.IsNullOrEmpty(urlString)
                    || urlString
                        .TrimStart()
                        .StartsWith("{field", StringComparison.OrdinalIgnoreCase)
                )
                    audioUrl = null;
            }

            if (audioUrl != null)
            {
                logger.LogInformation(
                    "Downloading audio from {Url} for PostId: {PostId}",
                    audioUrl,
                    postId
                );
                var audioPath = Path.Combine(workingDir, "audio.mp3");
                await using var stream = await httpClient.GetStreamAsync(
                    audioUrl,
                    cancellationToken
                );
                await using var fileStream = File.Create(audioPath);
                await stream.CopyToAsync(fileStream, cancellationToken);
                logger.LogInformation(
                    "Audio downloaded successfully. Path: {Path}, PostId: {PostId}",
                    audioPath,
                    postId
                );
            }

            var options = new RenderOptions(
                job.Title ?? "Unknown Title",
                job.Location ?? "Unknown Location",
                job.Jugglers ?? "Unknown Jugglers",
                job.Musicartist ?? "Unknown Artist",
                job.BlockSpacing,
                job.InternalSpacing
            );

            logger.LogInformation("Starting render process for PostId: {PostId}", postId);

            // Perform render and upload
            var renderer = new Renderer();
            var result = await renderer.PerformRenderAndUploadAsync(
                workingDir,
                options,
                logger1,
                wordPressService,
                postId,
                postType,
                cancellationToken
            );

            if (result.Success)
            {
                logger.LogInformation(
                    "Render and upload completed successfully for PostId: {PostId}",
                    postId
                );

                // Delete job file after successful upload
                logger.LogInformation(
                    "Deleting job file after successful upload: {JobFile}",
                    jobFilePath
                );
                File.Delete(jobFilePath);
                logger.LogInformation("Job file deleted successfully: {JobFile}", jobFilePath);
            }
            else
            {
                logger.LogError(
                    "Render and upload failed for PostId: {PostId}, Error: {Error}",
                    postId,
                    result.ErrorMessage
                );
                // Keep the job file for retry
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error during render and upload process for PostId: {PostId}",
                postId
            );
            // Keep the job file for retry
            throw;
        }
        finally
        {
            // Cleanup working directory
            try
            {
                if (Directory.Exists(workingDir))
                {
                    logger.LogInformation(
                        "Cleaning up working directory: {WorkingDir} for PostId: {PostId}",
                        workingDir,
                        postId
                    );
                    Directory.Delete(workingDir, recursive: true);
                    logger.LogInformation(
                        "Working directory deleted: {WorkingDir} for PostId: {PostId}",
                        workingDir,
                        postId
                    );
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to cleanup working directory: {WorkingDir} for PostId: {PostId}",
                    workingDir,
                    postId
                );
            }
        }
    }

    /// <summary>
    /// Extracts the post ID and post type from a WordPress URI.
    /// Expected format: https://passing.zone/?post_type=pattern&p=2986
    /// </summary>
    private static bool TryExtractPostIdFromUri(
        string postIdUri,
        out int postId,
        out string postType
    )
    {
        postId = 0;
        postType = "posts"; // Default to "posts" if not specified

        if (string.IsNullOrWhiteSpace(postIdUri))
            return false;

        // Try to parse as URI
        if (!Uri.TryCreate(postIdUri, UriKind.Absolute, out var uri))
        {
            return false;
        }

        // Extract query parameters manually
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
        bool foundPostId = false;

        foreach (var param in parameters)
        {
            var parts = param.Split('=', 2);
            if (parts.Length == 2)
            {
                var key = parts[0];
                var value = Uri.UnescapeDataString(parts[1]);

                if (key.Equals("p", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(value, out postId))
                    {
                        foundPostId = true;
                    }
                }
                else if (key.Equals("post_type", StringComparison.OrdinalIgnoreCase))
                {
                    postType = value;
                }
            }
        }

        return foundPostId;
    }
}
