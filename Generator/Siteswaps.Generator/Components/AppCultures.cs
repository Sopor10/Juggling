namespace Siteswaps.Generator.Components;

/// <summary>Supported UI cultures for the WASM client (English keys, German translations).</summary>
public static class AppCultures
{
    public const string German = "de";
    public const string English = "en";

    /// <summary>Fallback when neither a stored preference nor the browser language is supported.</summary>
    public const string Default = English;

    public const string StorageKey = "BlazorCulture";

    public static readonly IReadOnlyList<string> Supported = [German, English];

    /// <summary>
    /// Maps a culture name (e.g. <c>de-DE</c>, <c>en</c>) to a supported UI culture.
    /// Unsupported languages fall back to <see cref="Default"/>.
    /// </summary>
    public static string Normalize(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return Default;
        }

        var primary = culture.Split('-', '_')[0].Trim().ToLowerInvariant();
        return primary switch
        {
            German => German,
            English => English,
            _ => Default,
        };
    }
}
