using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.WizardPage;

/// <summary>localStorage payload for cached wizard generation results, with a 7-day TTL.</summary>
internal sealed class WizardGenerationCacheEntry
{
    public static readonly TimeSpan TimeToLive = TimeSpan.FromDays(7);

    public DateTimeOffset StoredAtUtc { get; init; }

    public List<string> Results { get; init; } = [];

    public static WizardGenerationCacheEntry FromResults(
        IReadOnlyList<Siteswap> results,
        DateTimeOffset? nowUtc = null
    ) =>
        new()
        {
            StoredAtUtc = nowUtc ?? DateTimeOffset.UtcNow,
            Results = results.Select(r => r.ToString()).ToList(),
        };

    public bool IsExpired(DateTimeOffset? nowUtc = null) =>
        (nowUtc ?? DateTimeOffset.UtcNow) - StoredAtUtc >= TimeToLive;
}
