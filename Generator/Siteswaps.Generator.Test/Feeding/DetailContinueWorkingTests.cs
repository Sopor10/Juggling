using FluentAssertions;

namespace Siteswaps.Generator.Test.Feeding;

/// <summary>
/// Detail-page entry into the feeding route (Components markup contracts).
/// </summary>
[TestFixture]
public class DetailContinueWorkingTests
{
    [Test]
    public void DetailVariantHero_Offers_Three_Person_Feed_As_Relative_Link()
    {
        var hero = ReadComponentsSource(
            Path.Combine("Details", "Variants", "DetailVariantHero.razor")
        );

        hero.Should().Contain("Continue with this siteswap");
        hero.Should().Contain("Create 3-person feed");

        hero.Should().Contain("ThreePersonFeedHref");
        // App-relative: no leading slash on feeding path in markup or helper usage.
        hero.Should().NotContain("href=\"/feeding");
        hero.Should().NotContain("href='/feeding");

        var primaryStart = hero.IndexOf("sdv-continue-primary", StringComparison.Ordinal);
        var primaryEnd = hero.IndexOf(
            "sdv-continue-secondary",
            primaryStart,
            StringComparison.Ordinal
        );
        var primaryBlock = hero[primaryStart..primaryEnd];
        primaryBlock.Should().NotContain("target=\"_blank\"");
        primaryBlock.Should().NotContain("rel=\"noopener\"");
    }

    [Test]
    public void DetailVariantHero_Includes_Passist_Secondary_Action()
    {
        var hero = ReadComponentsSource(
            Path.Combine("Details", "Variants", "DetailVariantHero.razor")
        );

        hero.Should().Contain("Passist");
        hero.Should().Contain("PassistLink");
        hero.Should().Contain("Open in Passist");
    }

    [Test]
    public void DetailPage_Header_No_Longer_Hosts_Passist()
    {
        var page = ReadComponentsSource(Path.Combine("Details", "DetailPage.razor"));

        // Passist moved into the continue-working section on the hero.
        var headerBlock = ExtractUntil(page, "sd-header", "sd-sheet");
        headerBlock.Should().NotContain("PassistLink");
        headerBlock.Should().NotContain(">Passist<");
    }

    [Test]
    public void DetailVariantHero_Gates_Continue_Section_On_Eligibility()
    {
        var hero = ReadComponentsSource(
            Path.Combine("Details", "Variants", "DetailVariantHero.razor")
        );

        hero.Should().Contain("CanCreateThreePersonFeed");
    }

    private static string ExtractUntil(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }

    private static string ReadComponentsSource(string relativePathUnderComponents) =>
        File.ReadAllText(
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..",
                "..",
                "..",
                "..",
                "..",
                "Siteswaps.Components",
                relativePathUnderComponents
            )
        );
}
