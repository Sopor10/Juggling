namespace Siteswaps.Components.Details;

/// <summary>
/// JSON contract for localStorage key "settings".
/// Keep property names in sync with Siteswaps.Generator.Components.SettingsDto.
/// </summary>
public sealed class PersistedSettings
{
    public const string StorageKey = "settings";

    public bool ShowThrowNames { get; set; } = true;
    public int MaxHeight { get; set; } = 13;
    public string ThrowDisplayMode { get; set; } = "Global";

    public static DetailViewModel.ThrowDisplayMode ParseThrowDisplayMode(string? value) =>
        Enum.TryParse<DetailViewModel.ThrowDisplayMode>(value, ignoreCase: true, out var mode)
            ? mode
            : DetailViewModel.ThrowDisplayMode.Global;
}
