using System.Text.Json;
using System.Text.Json.Serialization;

public record Input
{
    /// <summary>WordPress post ID as number, numeric string, or full URI with ?p=.</summary>
    [JsonPropertyName("post_id")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? PostId { get; init; }

    [JsonPropertyName("video_url")]
    public string? VideoUrl { get; init; }

    /// <summary>Gravity Forms alias for the uploaded raw video.</summary>
    [JsonPropertyName("raw_video_url")]
    public string? RawVideoUrl { get; init; }

    /// <summary>Optional. When omitted, the original video audio is kept in the rendered output.</summary>
    [JsonPropertyName("audio_url")]
    public string? AudioUrl { get; init; }

    /// <summary>Gravity Forms alias for an optional replacement audio file.</summary>
    [JsonPropertyName("audio_file_url")]
    public string? AudioFileUrl { get; init; }

    public string? Location { get; init; }
    public string? Title { get; init; }

    /// <summary>Legacy Ninja Forms field for juggler names.</summary>
    public string? Jugglers { get; init; }

    /// <summary>Gravity Forms multi-select of jugglers (string or string array).</summary>
    [JsonPropertyName("monkeys")]
    public JsonElement? Monkeys { get; init; }

    [JsonPropertyName("monkeys_unlisted")]
    public string? MonkeysUnlisted { get; init; }

    [JsonPropertyName("musicartist")]
    public string? Musicartist { get; init; }

    [JsonPropertyName("music_attribution")]
    public string? MusicAttribution { get; init; }

    public string? ResolveVideoUrl() => FirstNonEmpty(RawVideoUrl, VideoUrl);

    public string? ResolveAudioUrl() => FirstNonEmpty(AudioFileUrl, AudioUrl);

    public string? ResolveMusicArtist() => FirstNonEmpty(MusicAttribution, Musicartist);

    public string? ResolveJugglers()
    {
        var names = new List<string>();

        if (Monkeys is { ValueKind: JsonValueKind.Array } monkeysArray)
        {
            foreach (var item in monkeysArray.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                    continue;

                var name = item.GetString();
                if (!string.IsNullOrWhiteSpace(name))
                    names.Add(name.Trim());
            }
        }
        else if (Monkeys is { ValueKind: JsonValueKind.String } monkeysString)
        {
            var single = monkeysString.GetString();
            if (!string.IsNullOrWhiteSpace(single))
                names.Add(single.Trim());
        }

        if (!string.IsNullOrWhiteSpace(MonkeysUnlisted))
            names.Add(MonkeysUnlisted.Trim());

        if (!string.IsNullOrWhiteSpace(Jugglers))
            names.Add(Jugglers.Trim());

        return names.Count == 0 ? null : string.Join(", ", names);
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}

/// <summary>Accepts JSON strings or numbers as string values.</summary>
file sealed class FlexibleStringConverter : JsonConverter<string?>
{
    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.String)
            return reader.GetString();

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out var number))
                return number.ToString();
            return reader.GetDouble().ToString();
        }

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        throw new JsonException($"Unexpected token type {reader.TokenType} when reading a string.");
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value);
    }
}
