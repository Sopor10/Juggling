using System.Text.Json.Serialization;

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
