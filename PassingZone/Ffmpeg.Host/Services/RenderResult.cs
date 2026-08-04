namespace Ffmpeg.Host.Services;

public class RenderResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? MediaId { get; set; }
    public string? VideoUrl { get; set; }
    public long? VideoSizeBytes { get; set; }
    public string? PatternUrl { get; set; }
}
