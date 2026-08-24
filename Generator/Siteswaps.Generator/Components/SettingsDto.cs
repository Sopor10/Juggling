namespace Siteswaps.Generator.Components;

/// <summary>Persisted user preferences (localStorage key "settings").</summary>
public record SettingsDto
{
    public const int MinMaxHeight = 1;
    public const int MaxMaxHeight = 50;

    public bool ShowThrowNames { get; set; } = true;
    public int MaxHeight { get; set; } = 13;

    /// <summary>Local, Global, or Name — default Global.</summary>
    public string ThrowDisplayMode { get; set; } = "Global";

    /// <summary>
    /// UI culture: <c>de</c> or <c>en</c>. Empty until the user saves a preference;
    /// the runtime then follows the browser language.
    /// </summary>
    public string Culture { get; set; } = "";

    public static int ClampMaxHeight(int maxHeight) =>
        Math.Clamp(maxHeight, MinMaxHeight, MaxMaxHeight);
}
