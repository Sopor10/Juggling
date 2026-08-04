using System.Diagnostics;
using System.Text;
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

            var display = ToAsciiDisplayText(options);

            await File.WriteAllTextAsync(Path.Combine(workingDir, "title.txt"), display.Title, ct);

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

            startInfo.EnvironmentVariables["TITLE"] = display.Title;
            startInfo.EnvironmentVariables["LOCATION"] = display.Location;
            startInfo.EnvironmentVariables["JUGGLERS"] = display.Jugglers;
            startInfo.EnvironmentVariables["MUSICARTIST"] = display.MusicArtist;

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

                    var upload = await wordPressService.UploadVideoAsync(
                        videoPath,
                        uploadFileName,
                        ct
                    );
                    // Default to "posts" if post type is not available in this context
                    await wordPressService.UpdatePostWithVideoAsync(
                        postId.Value,
                        upload.SourceUrl,
                        "posts",
                        ct
                    );

                    logger.LogInformation(
                        "Video successfully uploaded to WordPress. Media ID: {MediaId}, URL: {Url}",
                        upload.MediaId,
                        upload.SourceUrl
                    );

                    return TypedResults.Ok(
                        new
                        {
                            success = true,
                            mediaId = upload.MediaId,
                            videoUrl = upload.SourceUrl,
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
        CancellationToken ct,
        Action<JobStatus>? onStatus = null
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

            var display = ToAsciiDisplayText(options);

            await File.WriteAllTextAsync(Path.Combine(workingDir, "title.txt"), display.Title, ct);
            logger.LogDebug("Title file written: {Title}", display.Title);

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

            startInfo.EnvironmentVariables["TITLE"] = display.Title;
            startInfo.EnvironmentVariables["LOCATION"] = display.Location;
            startInfo.EnvironmentVariables["JUGGLERS"] = display.Jugglers;
            startInfo.EnvironmentVariables["MUSICARTIST"] = display.MusicArtist;

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
            onStatus?.Invoke(JobStatus.Uploading);
            logger.LogInformation("Uploading video to WordPress for post {PostId}", postId);

            try
            {
                // Generate filename: title-postId.mp4
                var sanitizedTitle = SanitizeFileName(options.Title);
                var uploadFileName = $"{sanitizedTitle}-{postId}.mp4";
                logger.LogInformation(
                    "Generated upload filename: {FileName} from original title: '{OriginalTitle}' for PostId: {PostId}",
                    uploadFileName,
                    options.Title,
                    postId
                );

                var upload = await wordPressService.UploadVideoAsync(videoPath, uploadFileName, ct);
                logger.LogInformation(
                    "Video uploaded to WordPress. MediaId: {MediaId}, URL: {Url}, PostId: {PostId}, FileName: {FileName}",
                    upload.MediaId,
                    upload.SourceUrl,
                    postId,
                    uploadFileName
                );

                await wordPressService.UpdatePostWithVideoAsync(
                    postId,
                    upload.SourceUrl,
                    postType,
                    ct
                );
                logger.LogInformation(
                    "Post updated with video. MediaId: {MediaId}, PostId: {PostId}",
                    upload.MediaId,
                    postId
                );

                return new RenderResult
                {
                    Success = true,
                    MediaId = upload.MediaId,
                    VideoUrl = upload.SourceUrl,
                    VideoSizeBytes = fileInfo.Length,
                };
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
    /// Replaces German umlauts for drawtext fonts that lack those glyphs.
    /// </summary>
    private static RenderOptions ToAsciiDisplayText(RenderOptions options) =>
        options with
        {
            Title = ReplaceGermanUmlauts(options.Title),
            Location = ReplaceGermanUmlauts(options.Location),
            Jugglers = ReplaceGermanUmlauts(options.Jugglers),
            MusicArtist = ReplaceGermanUmlauts(options.MusicArtist),
        };

    private static string ReplaceGermanUmlauts(string value) =>
        value
            .Replace("Ä", "Ae", StringComparison.Ordinal)
            .Replace("Ö", "Oe", StringComparison.Ordinal)
            .Replace("Ü", "Ue", StringComparison.Ordinal)
            .Replace("ä", "ae", StringComparison.Ordinal)
            .Replace("ö", "oe", StringComparison.Ordinal)
            .Replace("ü", "ue", StringComparison.Ordinal)
            .Replace("ß", "ss", StringComparison.Ordinal);

    /// <summary>
    /// Sanitizes a string to be used as a filename by removing/replacing invalid characters.
    /// Replaces non-ASCII characters with their closest ASCII equivalents using Unicode normalization.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "video";

        // Normalize to FormD (decomposes characters like 'ü' into 'u' and '¨')
        var normalizedString = fileName.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                // Only keep ASCII alphanumeric characters, hyphens, and underscores
                if (
                    (c >= 'a' && c <= 'z')
                    || (c >= 'A' && c <= 'Z')
                    || (c >= '0' && c <= '9')
                    || c == '-'
                    || c == '_'
                )
                {
                    stringBuilder.Append(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    stringBuilder.Append('-');
                }
            }
        }

        var sanitized = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

        // Replace multiple hyphens/underscores with single ones
        while (sanitized.Contains("--"))
            sanitized = sanitized.Replace("--", "-");
        while (sanitized.Contains("__"))
            sanitized = sanitized.Replace("__", "_");
        while (sanitized.Contains("-_"))
            sanitized = sanitized.Replace("-_", "-");
        while (sanitized.Contains("_-"))
            sanitized = sanitized.Replace("_-", "_");

        sanitized = sanitized.Trim('-', '_');

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
