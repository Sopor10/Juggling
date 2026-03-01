using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Ffmpeg.Host.Services;

public class WordPressOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ApplicationPassword { get; set; } = string.Empty;
}

public class WordPressService
{
    private readonly HttpClient _httpClient;
    private readonly WordPressOptions _options;
    private readonly ILogger<WordPressService> _logger;

    public WordPressService(
        HttpClient httpClient,
        IOptions<WordPressOptions> options,
        ILogger<WordPressService> logger
    )
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        // Configure Basic Authentication
        var authValue = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_options.Username}:{_options.ApplicationPassword}")
        );
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            authValue
        );
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<string> UploadVideoAsync(
        string videoPath,
        string? fileName = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation("Uploading video from {VideoPath} to WordPress", videoPath);

            if (!File.Exists(videoPath))
            {
                throw new FileNotFoundException($"Video file not found: {videoPath}");
            }

            // Use provided fileName or fallback to original filename
            var uploadFileName = fileName ?? Path.GetFileName(videoPath);
            _logger.LogInformation("Uploading video with filename: {FileName}", uploadFileName);

            var fileContent = await File.ReadAllBytesAsync(videoPath, cancellationToken);

            using var content = new MultipartFormDataContent();
            using var fileStreamContent = new ByteArrayContent(fileContent);
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
            content.Add(fileStreamContent, "file", uploadFileName);

            var response = await _httpClient.PostAsync(
                "/wp-json/wp/v2/media",
                content,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "WordPress upload failed with status {StatusCode}: {Error}",
                    response.StatusCode,
                    errorContent
                );
                throw new HttpRequestException(
                    $"WordPress upload failed: {response.StatusCode} - {errorContent}"
                );
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var mediaResponse = JsonSerializer.Deserialize<MediaResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (mediaResponse?.Id == null)
            {
                throw new InvalidOperationException(
                    "WordPress API returned invalid response: missing media ID"
                );
            }

            _logger.LogInformation(
                "Video uploaded successfully. Media ID: {MediaId}, URL: {Url}",
                mediaResponse.Id,
                mediaResponse.SourceUrl
            );

            if (mediaResponse.SourceUrl is null)
            {
                throw new InvalidOperationException("Media URL is null after upload.");
            }
            return mediaResponse.SourceUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading video to WordPress");
            throw;
        }
    }

    public async Task UpdatePostWithVideoAsync(
        int postId,
        string sourceUrl,
        string postType,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Updating ACF fields for {PostType} {PostId} with video Url {MediaId}",
                postType,
                postId,
                sourceUrl
            );

            // Update ACF fields for Presto Player
            // According to https://rudrastyh.com/wordpress/add-meta-fields-with-rest-api.html
            // we should use 'acf' instead of 'meta' for ACF fields
            var updatePayload = new
            {
                acf = new Dictionary<string, object>
                {
                    ["presto_shortcode"] = $"[presto_player src=\"{sourceUrl}\" preset=6]",
                },
            };

            _logger.LogDebug(
                "Updating ACF fields: presto_shortcode={MediaId}, PostType={PostType}",
                sourceUrl,
                postType
            );

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(updatePayload),
                Encoding.UTF8,
                "application/json"
            );

            // Use the correct endpoint for the post type
            // For custom post types: /wp-json/wp/v2/{post_type}/{id}
            // For standard posts: /wp-json/wp/v2/posts/{id}
            var endpoint = $"/wp-json/wp/v2/{postType}/{postId}";
            _logger.LogDebug("Using WordPress REST API endpoint: {Endpoint}", endpoint);

            var updateResponse = await _httpClient.PutAsync(
                endpoint,
                jsonContent,
                cancellationToken
            );

            if (!updateResponse.IsSuccessStatusCode)
            {
                var errorContent = await updateResponse.Content.ReadAsStringAsync(
                    cancellationToken
                );
                _logger.LogError(
                    "Failed to update ACF fields for {PostType} {PostId}: {StatusCode} - {Error}",
                    postType,
                    postId,
                    updateResponse.StatusCode,
                    errorContent
                );
                throw new HttpRequestException(
                    $"Failed to update ACF fields: {updateResponse.StatusCode} - {errorContent}"
                );
            }

            _logger.LogInformation(
                "ACF fields updated successfully for {PostType} {PostId} with media ID {MediaId}",
                postType,
                postId,
                sourceUrl
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ACF fields for post with video");
            throw;
        }
    }

    private class MediaResponse
    {
        public int? Id { get; set; }
        
        [JsonPropertyName("source_url")]
        public string? SourceUrl { get; set; }
    }
}
