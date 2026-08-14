using FluentAssertions;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Design;

/// <summary>Asserts Passing Zone CTA orange and cyan focus language on /wizard (mockup pz-btn-primary = orange).</summary>
[Collection(WizardE2ECollection.Name)]
public class WizardCtaFocusTests(BlazorWebassemblyFixture<Program> fixture)
{
    /// <summary>Summary: Forward CTA (Weiter) must use brand orange #f9a500 with dark purple text, not purple fill or Bootstrap primary.</summary>
    [Fact]
    public async Task Wizard_ForwardCta_UsesBrandOrangeNotPurpleFill()
    {
        var design = await WizardDesignPage.OpenAsync(fixture);
        var button = design.PrimaryForwardButton;
        await button.WaitForAsync();

        var backgroundImage = await design.StyleAsync(button, "background-image");
        var backgroundColor = await design.StyleAsync(button, "background-color");
        var color = await design.StyleAsync(button, "color");
        var combinedBg = $"{backgroundImage} {backgroundColor}";

        (
            DesignColor.CssContainsHex(combinedBg, DesignColor.BrandOrange)
            || DesignColor.EqualsHex(backgroundColor, DesignColor.BrandOrange)
        )
            .Should()
            .BeTrue(
                $"Weiter CTA must use brand orange {DesignColor.BrandOrange}, got bg={combinedBg}"
            );

        DesignColor
            .EqualsHex(backgroundColor, DesignColor.BrandPurple700)
            .Should()
            .BeFalse("Weiter must not be flat purple-700; mockup pz-btn-primary is orange");

        DesignColor
            .EqualsHex(color, DesignColor.BrandPurple950)
            .Should()
            .BeTrue(
                $"orange CTA text must be dark purple {DesignColor.BrandPurple950}, got {color}"
            );

        DesignColor.CssContainsHex(combinedBg, DesignColor.LegacyMaterialPurple).Should().BeFalse();
    }

    /// <summary>Summary: Generate CTA must use orange gradient #f9a500 with purple-950 text for contrast.</summary>
    [Fact]
    public async Task Wizard_GenerateCta_UsesOrangeGradientWithDarkPurpleText()
    {
        var design = await WizardDesignPage.OpenAsync(fixture);
        await design.Wizard.ClickNextAsync();
        await design.Wizard.ClickNextAsync();
        await design.GenerateButton.WaitForAsync();

        var backgroundImage = await design.StyleAsync(design.GenerateButton, "background-image");
        var backgroundColor = await design.StyleAsync(design.GenerateButton, "background-color");
        var color = await design.StyleAsync(design.GenerateButton, "color");
        var combinedBg = $"{backgroundImage} {backgroundColor}";

        DesignColor
            .CssContainsHex(combinedBg, DesignColor.BrandOrange)
            .Should()
            .BeTrue(
                $"Generate CTA must include orange {DesignColor.BrandOrange}, got {combinedBg}"
            );

        DesignColor
            .EqualsHex(color, DesignColor.BrandPurple950)
            .Should()
            .BeTrue(
                $"Generate CTA text must be {DesignColor.BrandPurple950} for contrast, got {color}"
            );
    }

    /// <summary>Summary: Keyboard focus ring on wizard controls must be cyan #00b3ff, not browser default or Material blue.</summary>
    [Fact]
    public async Task Wizard_FocusVisible_UsesBrandCyanOutline()
    {
        var design = await WizardDesignPage.OpenAsync(fixture);
        var button = design.PrimaryForwardButton;
        await design.FocusVisibleAsync(button);

        var outlineColor = await design.StyleAsync(button, "outline-color");
        var outlineStyle = await design.StyleAsync(button, "outline-style");
        var outlineWidth = DesignColor.ParseCssPx(await design.StyleAsync(button, "outline-width"));

        outlineStyle.Should().NotBe("none");
        outlineWidth.Should().BeGreaterThanOrEqualTo(2);
        DesignColor
            .EqualsHex(outlineColor, DesignColor.BrandCyan)
            .Should()
            .BeTrue($"focus outline must be cyan {DesignColor.BrandCyan}, got {outlineColor}");
    }

    /// <summary>Summary: Ghost Back control stays soft lavender purple-100, not gray Bootstrap secondary.</summary>
    [Fact]
    public async Task Wizard_GhostBack_UsesLavenderPurpleNotGray()
    {
        var design = await WizardDesignPage.OpenAsync(fixture);
        await design.Wizard.ClickNextAsync();
        await design.GhostBackButton.WaitForAsync();

        var background = await design.StyleAsync(design.GhostBackButton, "background-color");
        var color = await design.StyleAsync(design.GhostBackButton, "color");

        DesignColor
            .EqualsHex(background, DesignColor.BrandPurple100)
            .Should()
            .BeTrue($"ghost back bg must be {DesignColor.BrandPurple100}, got {background}");
        DesignColor
            .EqualsHex(color, DesignColor.BrandPurple700)
            .Should()
            .BeTrue($"ghost back text must be {DesignColor.BrandPurple700}, got {color}");
    }
}
