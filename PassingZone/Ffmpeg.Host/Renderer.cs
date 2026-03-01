using System.Diagnostics;
using Ffmpeg.Host.Services;

public class Renderer
{
    public async Task<IResult> PerformRenderAsync(
        string workingDir,
        RenderOptions options,
        ILogger logger,
        CancellationToken ct
    )
    {
        return await PerformRenderAsync(workingDir, options, logger, null, null, ct);
    }

    public async Task<IResult> PerformRenderAsync(
        string workingDir,
        RenderOptions options,
        ILogger logger,
        WordPressService? wordPressService,
        int? postId,
        CancellationToken ct
    )
    {
        try
        {
            logger.LogInformation(
                "Directory content: {Files}",
                string.Join(", ", Directory.GetFiles(workingDir).Select(Path.GetFileName))
            );

            await File.WriteAllTextAsync(Path.Combine(workingDir, "title.txt"), options.Title, ct);

            var scriptPath = Path.Combine(
                AppContext.BaseDirectory,
                "Reference-Adrian",
                "render.sh"
            );
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"\"{scriptPath}\" \"{workingDir}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AppContext.BaseDirectory,
            };

            startInfo.EnvironmentVariables["TITLE"] = options.Title;
            startInfo.EnvironmentVariables["LOCATION"] = options.Location;
            startInfo.EnvironmentVariables["JUGGLERS"] = options.Jugglers;
            startInfo.EnvironmentVariables["MUSICARTIST"] = options.MusicArtist;

            if (!string.IsNullOrEmpty(options.BlockSpacing))
            {
                startInfo.EnvironmentVariables["BLOCK_SPACING"] = options.BlockSpacing;
            }

            if (!string.IsNullOrEmpty(options.InternalSpacing))
            {
                startInfo.EnvironmentVariables["INTERNAL_SPACING"] = options.InternalSpacing;
            }

            using var process = new Process { StartInfo = startInfo };
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    logger.LogInformation("[render.sh OUT] {Data}", e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    logger.LogError("[render.sh ERR] {Data}", e.Data);
                }
            };

            if (!process.Start())
            {
                logger.LogError("Failed to start render process");
                return Results.Problem("Failed to start render process.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(ct);

            if (process.ExitCode != 0)
            {
                return Results.Problem($"Render process failed with exit code {process.ExitCode}.");
            }

            var videoPath = Path.Combine(workingDir, "rendered_output.mp4");

            if (!File.Exists(videoPath))
            {
                logger.LogWarning("rendered_output.mp4 not found. Searching for any mp4...");
                // Fallback: search for any mp4 in the directory (excluding the input video)
                var mp4Files = Directory
                    .GetFiles(workingDir, "*.mp4")
                    .Where(f =>
                        !Path.GetFileName(f).Equals("video.mp4", StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();

                if (mp4Files.Count > 0)
                {
                    videoPath = mp4Files[0];
                }
                else
                {
                    return Results.Problem("Output video file not found.");
                }
            }

            var fileInfo = new FileInfo(videoPath);
            logger.LogInformation(
                "Generated video: {VideoPath} ({Size} bytes)",
                videoPath,
                fileInfo.Length
            );

            if (fileInfo.Length == 0)
            {
                return Results.Problem("Generated video file is empty.");
            }

            // If WordPressService and postId are provided, upload to WordPress
            if (wordPressService != null && postId.HasValue)
            {
                logger.LogInformation(
                    "Uploading video to WordPress for post {PostId}",
                    postId.Value
                );

                try
                {
                    // Generate filename: title-postId.mp4
                    var sanitizedTitle = SanitizeFileName(options.Title);
                    var uploadFileName = $"{sanitizedTitle}-{postId.Value}.mp4";
                    logger.LogInformation(
                        "Uploading video with filename: {FileName} for PostId: {PostId}",
                        uploadFileName,
                        postId.Value
                    );

                    var mediaId = await wordPressService.UploadVideoAsync(
                        videoPath,
                        uploadFileName,
                        ct
                    );
                    // Default to "posts" if post type is not available in this context
                    await wordPressService.UpdatePostWithVideoAsync(
                        postId.Value,
                        mediaId,
                        "posts",
                        ct
                    );

                    logger.LogInformation(
                        "Video successfully uploaded to WordPress. Media ID: {MediaId}",
                        mediaId
                    );

                    return TypedResults.Ok(
                        new
                        {
                            success = true,
                            mediaId = mediaId,
                            postId = postId.Value,
                            message = "Video uploaded and post updated successfully",
                        }
                    );
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to upload video to WordPress");
                    return TypedResults.Problem(
                        $"Failed to upload video to WordPress: {ex.Message}"
                    );
                }
            }

            // Fallback: Return video stream for /render endpoint
            var downloadName = $"{options.Title.Trim()}.mp4";
            logger.LogInformation("Opening stream for {VideoPath}", videoPath);
            var stream = new FileStream(
                videoPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true
            );

            return TypedResults.Stream(
                stream,
                "video/mp4",
                downloadName,
                enableRangeProcessing: true
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during rendering");
            return TypedResults.Problem($"An error occurred: {ex.Message}");
        }
    }

    public async Task<RenderResult> PerformRenderAndUploadAsync(
        string workingDir,
        RenderOptions options,
        ILogger logger,
        WordPressService wordPressService,
        int postId,
        string postType,
        CancellationToken ct
    )
    {
        try
        {
            logger.LogInformation(
                "Starting render and upload process. Directory: {WorkingDir}, PostId: {PostId}",
                workingDir,
                postId
            );

            logger.LogInformation(
                "Directory content: {Files}",
                string.Join(", ", Directory.GetFiles(workingDir).Select(Path.GetFileName))
            );

            await File.WriteAllTextAsync(Path.Combine(workingDir, "title.txt"), options.Title, ct);
            logger.LogDebug("Title file written: {Title}", options.Title);

            var scriptPath = Path.Combine(
                AppContext.BaseDirectory,
                "Reference-Adrian",
                "render.sh"
            );
            logger.LogDebug("Using render script: {ScriptPath}", scriptPath);

            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"\"{scriptPath}\" \"{workingDir}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AppContext.BaseDirectory,
            };

            startInfo.EnvironmentVariables["TITLE"] = options.Title;
            startInfo.EnvironmentVariables["LOCATION"] = options.Location;
            startInfo.EnvironmentVariables["JUGGLERS"] = options.Jugglers;
            startInfo.EnvironmentVariables["MUSICARTIST"] = options.MusicArtist;

            if (!string.IsNullOrEmpty(options.BlockSpacing))
            {
                startInfo.EnvironmentVariables["BLOCK_SPACING"] = options.BlockSpacing;
            }

            if (!string.IsNullOrEmpty(options.InternalSpacing))
            {
                startInfo.EnvironmentVariables["INTERNAL_SPACING"] = options.InternalSpacing;
            }

            logger.LogInformation("Starting render process for PostId: {PostId}", postId);

            using var process = new Process { StartInfo = startInfo };
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    logger.LogInformation("[render.sh OUT] {Data}", e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    logger.LogError("[render.sh ERR] {Data}", e.Data);
                }
            };

            if (!process.Start())
            {
                logger.LogError("Failed to start render process for PostId: {PostId}", postId);
                return new RenderResult
                {
                    Success = false,
                    ErrorMessage = "Failed to start render process.",
                };
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(ct);

            if (process.ExitCode != 0)
            {
                logger.LogError(
                    "Render process failed with exit code {ExitCode} for PostId: {PostId}",
                    process.ExitCode,
                    postId
                );
                return new RenderResult
                {
                    Success = false,
                    ErrorMessage = $"Render process failed with exit code {process.ExitCode}.",
                };
            }

            logger.LogInformation(
                "Render process completed successfully for PostId: {PostId}",
                postId
            );

            var videoPath = Path.Combine(workingDir, "rendered_output.mp4");

            if (!File.Exists(videoPath))
            {
                logger.LogWarning(
                    "rendered_output.mp4 not found. Searching for any mp4... PostId: {PostId}",
                    postId
                );
                // Fallback: search for any mp4 in the directory (excluding the input video)
                var mp4Files = Directory
                    .GetFiles(workingDir, "*.mp4")
                    .Where(f =>
                        !Path.GetFileName(f).Equals("video.mp4", StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();

                if (mp4Files.Count > 0)
                {
                    videoPath = mp4Files[0];
                    logger.LogInformation(
                        "Found alternative video file: {VideoPath} for PostId: {PostId}",
                        videoPath,
                        postId
                    );
                }
                else
                {
                    logger.LogError("Output video file not found for PostId: {PostId}", postId);
                    return new RenderResult
                    {
                        Success = false,
                        ErrorMessage = "Output video file not found.",
                    };
                }
            }

            var fileInfo = new FileInfo(videoPath);
            logger.LogInformation(
                "Generated video: {VideoPath} ({Size} bytes) for PostId: {PostId}",
                videoPath,
                fileInfo.Length,
                postId
            );

            if (fileInfo.Length == 0)
            {
                logger.LogError("Generated video file is empty for PostId: {PostId}", postId);
                return new RenderResult
                {
                    Success = false,
                    ErrorMessage = "Generated video file is empty.",
                };
            }

            // Upload to WordPress
            logger.LogInformation("Uploading video to WordPress for post {PostId}", postId);

            try
            {
                // Generate filename: title-postId.mp4
                var sanitizedTitle = SanitizeFileName(options.Title);
                var uploadFileName = $"{sanitizedTitle}-{postId}.mp4";
                logger.LogInformation(
                    "Uploading video with filename: {FileName} for PostId: {PostId}",
                    uploadFileName,
                    postId
                );

                var mediaId = await wordPressService.UploadVideoAsync(
                    videoPath,
                    uploadFileName,
                    ct
                );
                logger.LogInformation(
                    "Video uploaded to WordPress. MediaId: {MediaId}, PostId: {PostId}, FileName: {FileName}",
                    mediaId,
                    postId,
                    uploadFileName
                );

                await wordPressService.UpdatePostWithVideoAsync(postId, mediaId, postType, ct);
                logger.LogInformation(
                    "Post updated with video. MediaId: {MediaId}, PostId: {PostId}",
                    mediaId,
                    postId
                );

                return new RenderResult { Success = true, MediaId = mediaId };
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to upload video to WordPress for PostId: {PostId}",
                    postId
                );
                return new RenderResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to upload video to WordPress: {ex.Message}",
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred during render and upload for PostId: {PostId}",
                postId
            );
            return new RenderResult
            {
                Success = false,
                ErrorMessage = $"An error occurred: {ex.Message}",
            };
        }
    }

    /// <summary>
    /// Sanitizes a string to be used as a filename by removing/replacing invalid characters.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "video";

        // Remove or replace invalid filename characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join(
            "_",
            fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)
        );

        // Remove leading/trailing spaces and dots
        sanitized = sanitized.Trim().Trim('.');

        // Replace multiple spaces/underscores with single underscore
        while (sanitized.Contains("__"))
        {
            sanitized = sanitized.Replace("__", "_");
        }

        // Limit length to avoid filesystem issues
        if (sanitized.Length > 200)
        {
            sanitized = sanitized.Substring(0, 200);
        }

        // If empty after sanitization, use default
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "video";
        }

        return sanitized;
    }
}
