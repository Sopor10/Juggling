using System.Text.Json.Serialization;

namespace Ffmpeg.Host.Services;

public class RenderJob
{
    [JsonPropertyName("post_id")]
    public string? PostId { get; set; }

    [JsonPropertyName("video_url")]
    public Uri? Video { get; set; }

    [JsonPropertyName("audio_url")]
    public Uri? Audio { get; set; }

    public string? Location { get; set; }
    public string? Title { get; set; }
    public string? Jugglers { get; set; }

    [JsonPropertyName("musicartist")]
    public string? Musicartist { get; set; }

    [JsonPropertyName("blockSpacing")]
    public string? BlockSpacing { get; set; }

    [JsonPropertyName("internalSpacing")]
    public string? InternalSpacing { get; set; }
}
