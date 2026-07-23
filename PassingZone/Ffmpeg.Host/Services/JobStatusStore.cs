using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Ffmpeg.Host.Services;

public class JobStatusStore
{
    private readonly ConcurrentDictionary<int, JobInfo> _jobs = new();
    private readonly ILogger<JobStatusStore> _logger;
    private string _inputDirectory = string.Empty;
    private string _finishedDirectory = string.Empty;
    private string _logDirectory = string.Empty;

    public JobStatusStore(
        IOptions<RenderJobProcessorOptions> options,
        ILogger<JobStatusStore> logger
    )
    {
        _logger = logger;
        ConfigureDirectories(options.Value);
    }

    public string LogDirectory => _logDirectory;

    public void ConfigureDirectories(RenderJobProcessorOptions options)
    {
        _inputDirectory = string.IsNullOrEmpty(options.InputDirectory)
            ? Path.Combine(Path.GetTempPath(), "ffmpeg-jobs")
            : options.InputDirectory;

        _finishedDirectory = Path.IsPathRooted(options.FinishedDirectory)
            ? options.FinishedDirectory
            : Path.Combine(_inputDirectory, options.FinishedDirectory);

        _logDirectory = Path.Combine(_inputDirectory, "logs");
        Directory.CreateDirectory(_logDirectory);
    }

    public void Enqueue(int postId, string? title, string? patternUrl = null)
    {
        var now = DateTimeOffset.UtcNow;
        _jobs.AddOrUpdate(
            postId,
            _ => new JobInfo
            {
                PostId = postId,
                Title = title,
                PatternUrl = patternUrl,
                Status = JobStatus.Queued,
                QueuedAt = now,
                UpdatedAt = now,
            },
            (_, existing) =>
            {
                existing.Title = title ?? existing.Title;
                existing.PatternUrl = patternUrl ?? existing.PatternUrl;
                existing.Status = JobStatus.Queued;
                existing.QueuedAt = now;
                existing.UpdatedAt = now;
                existing.StartedAt = null;
                existing.FinishedAt = null;
                existing.LastError = null;
                existing.MediaId = null;
                existing.VideoUrl = null;
                existing.VideoSizeBytes = null;
                return existing;
            }
        );
    }

    public void SetStatus(int postId, JobStatus status, string? error = null)
    {
        var now = DateTimeOffset.UtcNow;
        _jobs.AddOrUpdate(
            postId,
            _ => new JobInfo
            {
                PostId = postId,
                Status = status,
                QueuedAt = now,
                UpdatedAt = now,
                StartedAt = status
                    is JobStatus.Downloading
                        or JobStatus.Rendering
                        or JobStatus.Uploading
                    ? now
                    : null,
                FinishedAt = status is JobStatus.Finished or JobStatus.Failed ? now : null,
                LastError = error,
            },
            (_, existing) =>
            {
                existing.Status = status;
                existing.UpdatedAt = now;
                if (
                    status is JobStatus.Downloading or JobStatus.Rendering or JobStatus.Uploading
                    && existing.StartedAt is null
                )
                {
                    existing.StartedAt = now;
                }

                if (status is JobStatus.Finished or JobStatus.Failed)
                {
                    existing.FinishedAt = now;
                }

                if (error is not null)
                {
                    existing.LastError = error;
                }
                else if (status != JobStatus.Failed)
                {
                    existing.LastError = null;
                }

                return existing;
            }
        );
    }

    public void SetResult(
        int postId,
        int mediaId,
        string videoUrl,
        long videoSizeBytes,
        string? patternUrl = null
    )
    {
        var now = DateTimeOffset.UtcNow;
        _jobs.AddOrUpdate(
            postId,
            _ => new JobInfo
            {
                PostId = postId,
                Status = JobStatus.Finished,
                QueuedAt = now,
                UpdatedAt = now,
                FinishedAt = now,
                MediaId = mediaId,
                VideoUrl = videoUrl,
                VideoSizeBytes = videoSizeBytes,
                PatternUrl = patternUrl,
            },
            (_, existing) =>
            {
                existing.MediaId = mediaId;
                existing.VideoUrl = videoUrl;
                existing.VideoSizeBytes = videoSizeBytes;
                if (patternUrl is not null)
                    existing.PatternUrl = patternUrl;
                existing.UpdatedAt = now;
                return existing;
            }
        );
    }

    public IReadOnlyList<JobInfo> GetAll() =>
        _jobs.Values.OrderByDescending(j => j.UpdatedAt).ToList();

    public JobInfo? Get(int postId) => _jobs.TryGetValue(postId, out var job) ? job : null;

    public string? GetLogPath(int postId)
    {
        var path = Path.Combine(_logDirectory, $"{postId}.log");
        return File.Exists(path) ? path : null;
    }

    public async Task<string> ReadLogsAsync(
        int postId,
        CancellationToken cancellationToken = default
    )
    {
        var path = Path.Combine(_logDirectory, $"{postId}.log");
        if (!File.Exists(path))
            return string.Empty;

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite
        );
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public void LoadFromDisk()
    {
        try
        {
            if (Directory.Exists(_inputDirectory))
            {
                foreach (var file in Directory.GetFiles(_inputDirectory, "*.json"))
                {
                    TryLoadJobFile(file, JobStatus.Queued);
                }
            }

            if (Directory.Exists(_finishedDirectory))
            {
                foreach (var file in Directory.GetFiles(_finishedDirectory, "*.json"))
                {
                    TryLoadJobFile(file, JobStatus.Finished);
                }
            }

            if (Directory.Exists(_logDirectory))
            {
                foreach (var file in Directory.GetFiles(_logDirectory, "*.log"))
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    if (!int.TryParse(name, out var postId))
                        continue;

                    if (_jobs.ContainsKey(postId))
                        continue;

                    var info = new FileInfo(file);
                    _jobs[postId] = new JobInfo
                    {
                        PostId = postId,
                        Status = JobStatus.Finished,
                        QueuedAt = info.CreationTimeUtc,
                        UpdatedAt = info.LastWriteTimeUtc,
                        FinishedAt = info.LastWriteTimeUtc,
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load existing jobs from disk");
        }
    }

    private void TryLoadJobFile(string filePath, JobStatus status)
    {
        try
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            // finished collisions: 2986_20260101_120000
            var idPart = fileName.Split('_')[0];
            if (!int.TryParse(idPart, out var postId))
                return;

            if (_jobs.ContainsKey(postId) && status == JobStatus.Queued)
                return;

            string? title = null;
            string? patternUrl = null;
            try
            {
                var json = File.ReadAllText(filePath);
                var job = JsonSerializer.Deserialize<RenderJob>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                title = job?.Title;
                patternUrl = job?.PostId;
            }
            catch
            {
                // ignore parse errors; still register the job
            }

            var info = new FileInfo(filePath);
            _jobs.AddOrUpdate(
                postId,
                _ => new JobInfo
                {
                    PostId = postId,
                    Title = title,
                    PatternUrl = patternUrl,
                    Status = status,
                    QueuedAt = info.CreationTimeUtc,
                    UpdatedAt = info.LastWriteTimeUtc,
                    FinishedAt = status == JobStatus.Finished ? info.LastWriteTimeUtc : null,
                },
                (_, existing) =>
                {
                    if (status == JobStatus.Finished || existing.Status == JobStatus.Queued)
                    {
                        existing.Status = status;
                        existing.Title ??= title;
                        existing.PatternUrl ??= patternUrl;
                        existing.UpdatedAt = info.LastWriteTimeUtc;
                        if (status == JobStatus.Finished)
                            existing.FinishedAt = info.LastWriteTimeUtc;
                    }

                    return existing;
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Skipping job file during load: {File}", filePath);
        }
    }
}
