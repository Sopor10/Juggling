using FluentAssertions;

namespace Siteswaps.Generator.Test.Feeding;

/// <summary>
/// Normal feed: A on TimeZone 0, B1/B2 on TimeZone 1 (half beat vs feeder, sync with each other).
/// Combination overview must stagger throw rows like Details <c>sdv-hero-throws</c>.
/// </summary>
[TestFixture]
public class FeedingBeatStaggerTests
{
    [Test]
    public void FeedingJugglerOverview_Applies_TimeZone_Stagger_Css_Vars()
    {
        var razor = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingJugglerOverview.razor")
        );

        razor
            .Should()
            .Contain(
                "--feeding-stagger:{row.Juggler}",
                "B1/B2 TimeZone must drive horizontal half-beat offset vs A"
            );
        razor
            .Should()
            .Contain(
                "--feeding-jugglers:{row.NumberOfJugglers}",
                "stagger pitch must scale by pair juggler count (same as Details)"
            );
    }

    [Test]
    public void FeedingPage_Css_Offsets_Throws_By_Stagger_Fraction()
    {
        var css = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingPage.razor.css")
        );

        css.Should()
            .Contain(
                "--feeding-stagger",
                "combination view must shift feeding-juggler-throws by TimeZone"
            );
        css.Should()
            .Contain(
                "(44px + 8px)",
                "stagger must use chip pitch (min-width + gap), matching Details"
            );
        css.Should()
            .Contain(
                "--feeding-jugglers",
                "half beat for 2-person pairs is stagger * pitch / jugglers"
            );
        css.Should()
            .NotContain(
                "margin-left: calc(",
                "margin and transform used together double the intended half-beat offset"
            );
        css.Split("var(--feeding-stagger", StringSplitOptions.None)
            .Should()
            .HaveCount(2, "the TimeZone offset must be applied exactly once");
    }

    [Test]
    public void Throw_Chips_Expose_Button_Selection_And_Landing_Status()
    {
        var razor = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingThrowChipRow.razor")
        );
        var overview = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingJugglerOverview.razor")
        );

        razor.Should().Contain("<button type=\"button\"");
        razor.Should().Contain("aria-pressed=");
        razor.Should().Contain("is-selected");
        razor.Should().Contain("is-landing");
        overview.Should().Contain("role=\"status\"");
        overview.Should().Contain("aria-live=\"polite\"");
    }

    private static string ReadGeneratorSource(string relativePathUnderGeneratorProject) =>
        File.ReadAllText(
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..",
                "..",
                "..",
                "..",
                "Siteswaps.Generator",
                relativePathUnderGeneratorProject
            )
        );
}
