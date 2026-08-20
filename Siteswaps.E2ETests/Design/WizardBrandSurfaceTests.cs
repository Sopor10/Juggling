using FluentAssertions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Design;

/// <summary>Asserts Passing Zone brand surfaces on /wizard: purple tokens, lavender wash, Baloo/Nunito, purple header — not Material #8E44AD.</summary>
public class WizardBrandSurfaceTests(SharedBlazorFixture host) : IClassFixture<SharedBlazorFixture>
{
    /// <summary>Summary: Wizard CSS vars must be brand purple/orange/lavender, never legacy Material #8E44AD.</summary>
    [Fact]
    public async Task Wizard_CssTokens_MatchPassingZoneBrand()
    {
        await using var design = await WizardDesignPage.OpenAsync(host.Fixture);

        DesignColor
            .NormalizeHex(await design.CssVarAsync("--wizard-purple-800"))
            .Should()
            .Be(DesignColor.NormalizeHex(DesignColor.BrandPurple800));
        DesignColor
            .NormalizeHex(await design.CssVarAsync("--wizard-purple-600"))
            .Should()
            .Be(DesignColor.NormalizeHex(DesignColor.BrandPurple600));
        DesignColor
            .NormalizeHex(await design.CssVarAsync("--wizard-purple-500"))
            .Should()
            .Be(DesignColor.NormalizeHex(DesignColor.BrandPurple500));
        DesignColor
            .NormalizeHex(await design.CssVarAsync("--wizard-orange"))
            .Should()
            .Be(DesignColor.NormalizeHex(DesignColor.BrandOrange));
        DesignColor
            .NormalizeHex(await design.CssVarAsync("--wizard-bg"))
            .Should()
            .Be(DesignColor.NormalizeHex(DesignColor.BrandLavenderBg));
    }

    /// <summary>Summary: Page wash must be lavender #f5f3fb, not flat gray Bootstrap base.</summary>
    [Fact]
    public async Task Wizard_PageBackground_IsLavenderBrandWash()
    {
        await using var design = await WizardDesignPage.OpenAsync(host.Fixture);
        var background = await design.StyleAsync(design.Wizard.Root, "background-color");
        DesignColor
            .EqualsHex(background, DesignColor.BrandLavenderBg)
            .Should()
            .BeTrue($"expected lavender {DesignColor.BrandLavenderBg}, got {background}");
    }

    /// <summary>Summary: Header must be deep brand-purple radial, not Material purple or flat gray.</summary>
    [Fact]
    public async Task Wizard_Header_UsesBrandPurpleGradient()
    {
        await using var design = await WizardDesignPage.OpenAsync(host.Fixture);
        var backgroundImage = await design.StyleAsync(design.Header, "background-image");
        var backgroundColor = await design.StyleAsync(design.Header, "background-color");
        var combined = $"{backgroundImage} {backgroundColor}";

        DesignColor
            .CssContainsHex(combined, DesignColor.BrandPurple800)
            .Should()
            .BeTrue(
                $"header should include brand purple {DesignColor.BrandPurple800}, got {combined}"
            );
        DesignColor
            .CssContainsHex(combined, DesignColor.LegacyMaterialPurple)
            .Should()
            .BeFalse(
                $"header must not use Material {DesignColor.LegacyMaterialPurple}, got {combined}"
            );
    }

    /// <summary>Summary: Display titles use Baloo 2; PZ brand mark is present; wizard body uses Nunito.</summary>
    [Fact]
    public async Task Wizard_Fonts_UseBalooDisplayAndNunitoBody()
    {
        await using var design = await WizardDesignPage.OpenAsync(host.Fixture);

        var pageFont = await design.StyleAsync(design.Wizard.Root, "font-family");
        pageFont.Should().ContainEquivalentOf("Nunito");

        var titleFont = await design.StyleAsync(design.DisplaySample, "font-family");
        titleFont.Should().ContainEquivalentOf("Baloo");

        await Assertions.Expect(design.Logo).ToBeVisibleAsync();
        await Assertions
            .Expect(design.Logo)
            .ToHaveAttributeAsync(
                "src",
                new System.Text.RegularExpressions.Regex("passing_zone_short_logo\\.svg")
            );
    }
}
