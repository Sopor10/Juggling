using FluentAssertions;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.Wizard;

[TestFixture]
public class WizardGenerationCacheEntryTests
{
    /// <summary>Summary: Fresh cache entries must not be treated as expired within the 7-day TTL.</summary>
    [Test]
    public void IsExpired_False_Before_Ttl()
    {
        var storedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var entry = WizardGenerationCacheEntry.FromResults(
            [Siteswap.CreateFromCorrect("5")],
            storedAt
        );

        entry.IsExpired(storedAt.AddDays(6).AddHours(23)).Should().BeFalse();
    }

    /// <summary>Summary: Cache entries older than 7 days must expire and be discarded.</summary>
    [Test]
    public void IsExpired_True_After_Ttl()
    {
        var storedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var entry = WizardGenerationCacheEntry.FromResults(
            [Siteswap.CreateFromCorrect("5")],
            storedAt
        );

        entry.IsExpired(storedAt.AddDays(7)).Should().BeTrue();
    }
}
