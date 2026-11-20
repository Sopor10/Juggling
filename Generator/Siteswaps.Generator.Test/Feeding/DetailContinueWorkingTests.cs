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

        hero.Should().Contain("Create 3-person feed");
        hero.Should().NotContain("Continue with this siteswap");
        hero.Should().NotContain("sdv-continue-lead");

        hero.Should().Contain("ThreePersonFeedHref");
        // App-relative: no leading slash on feeding path in markup or helper usage.
        hero.Should().NotContain("href=\"/feeding");
        hero.Should().NotContain("href='/feeding");

        hero.Should().NotContain("sdv-continue-secondary");
    }

    [Test]
    public void DetailVariantJif_Attributes_And_Thanks_Passist()
    {
        var jif = ReadComponentsSource(
            Path.Combine("Details", "Variants", "DetailVariantJif.razor")
        );

        jif.Should().Contain("https://juggle.ch/jif");
        jif.Should().Contain("Passist");
        jif.Should().Contain("Danke an Passist");
    }

    [Test]
    public void DetailPage_Header_No_Longer_Hosts_Passist()
    {
        var page = ReadComponentsSource(Path.Combine("Details", "DetailPage.razor"));

        // Passist is attributed on the JIF subpage.
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
