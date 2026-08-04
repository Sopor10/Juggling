namespace Ffmpeg.Host.Services;

public enum JobStatus
{
    Queued,
    Downloading,
    Rendering,
    Uploading,
    Finished,
    Failed,
}

public class JobInfo
{
    public required int PostId { get; init; }
    public string? Title { get; set; }
    public string? PatternUrl { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public DateTimeOffset QueuedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedAt { get; set; }
    public string? LastError { get; set; }
    public int? MediaId { get; set; }
    public string? VideoUrl { get; set; }
    public long? VideoSizeBytes { get; set; }
}
