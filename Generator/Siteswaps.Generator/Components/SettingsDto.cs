namespace Siteswaps.Generator.Components;

/// <summary>Persisted user preferences (localStorage key "settings").</summary>
public record SettingsDto
{
    public bool ShowThrowNames { get; set; } = true;
    public int MaxHeight { get; set; } = 13;

    /// <summary>Local, Global, or Name — default Global.</summary>
    public string ThrowDisplayMode { get; set; } = "Global";
}
