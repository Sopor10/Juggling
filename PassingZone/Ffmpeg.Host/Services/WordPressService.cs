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

        _logger.LogInformation(
            "WordPressService initialized with BaseUrl: {BaseUrl}, Username: {Username}",
            _options.BaseUrl,
            _options.Username
        );
    }

    public async Task<WordPressUploadResult> UploadVideoAsync(
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

            // Log file info
            var contentType = "video/mp4"; // Default MIME type
            _logger.LogInformation(
                "Uploading video with filename: {FileName}, size: {Size} bytes, content-type: {ContentType}",
                uploadFileName,
                new FileInfo(videoPath).Length,
                contentType
            );

            var fileContent = await File.ReadAllBytesAsync(videoPath, cancellationToken);

            using var content = new MultipartFormDataContent();
            using var fileStreamContent = new ByteArrayContent(fileContent);
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue(
                "form-data"
            )
            {
                Name = "\"file\"",
                FileName = $"\"{uploadFileName}\"",
            };
            content.Add(fileStreamContent);

            // Pretty /wp-json/ permalinks return Apache 404 on passing.zone; query form works.
            var requestUri = "?rest_route=/wp/v2/media";
            _logger.LogInformation(
                "Sending POST request to {BaseUrl}{RequestUri} with filename {FileName}",
                _httpClient.BaseAddress,
                requestUri,
                uploadFileName
            );

            var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var requestHeaders = _httpClient.DefaultRequestHeaders.ToString();
                var contentHeaders = content.Headers.ToString();
                var fileHeaders = fileStreamContent.Headers.ToString();

                _logger.LogError(
                    "WordPress upload failed with status {StatusCode}: {Error}. \nRequest Headers: {RequestHeaders}\nContent Headers: {ContentHeaders}\nFile Headers: {FileHeaders}",
                    response.StatusCode,
                    errorContent,
                    requestHeaders,
                    contentHeaders,
                    fileHeaders
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

            return new WordPressUploadResult
            {
                MediaId = mediaResponse.Id.Value,
                SourceUrl = mediaResponse.SourceUrl,
            };
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
                    ["_presto_shortcode"] = "field_68f239371a219",
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

            // Pretty /wp-json/ permalinks return Apache 404 on passing.zone; query form works.
            // Custom post types: ?rest_route=/wp/v2/{post_type}/{id}
            var endpoint = $"?rest_route=/wp/v2/{postType}/{postId}";
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
            _logger.LogInformation(
                await updateResponse.Content.ReadAsStringAsync(cancellationToken)
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

public class WordPressUploadResult
{
    public required int MediaId { get; init; }
    public required string SourceUrl { get; init; }
}
