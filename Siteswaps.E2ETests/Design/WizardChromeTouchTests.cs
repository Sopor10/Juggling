using FluentAssertions;
using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests.Design;

/// <summary>Asserts Passing Zone chrome: sheet radii, pill/chip language, orange progress, and primary CTA size (chip/stepper touch targets live in Ux).</summary>
public class WizardChromeTouchTests(SharedBlazorFixture host) : IClassFixture<SharedBlazorFixture>
{
    /// <summary>Summary: Content sheet over header must use large top radius (~22px), not flat Material seam.</summary>
    [Fact]
    public async Task Wizard_StepSheet_HasLargeRoundedTopOverHeader()
    {
        await using var design = await WizardDesignPage.OpenAsync(host.Fixture);
        var topLeft = DesignColor.ParseCssPx(
            await design.StyleAsync(design.StepSheet, "border-top-left-radius")
        );
        var topRight = DesignColor.ParseCssPx(
            await design.StyleAsync(design.StepSheet, "border-top-right-radius")
        );

        topLeft.Should().BeGreaterThanOrEqualTo(20);
        topRight.Should().BeGreaterThanOrEqualTo(20);
    }

    /// <summary>Summary: Active progress dot must be brand orange #f9a500 on purple header.</summary>
    [Fact]
    public async Task Wizard_ActiveProgressDot_IsBrandOrange()
    {
        await using var design = await WizardDesignPage.OpenAsync(host.Fixture);
        var background = await design.StyleAsync(design.ActiveProgressDot, "background-color");
        DesignColor
            .EqualsHex(background, DesignColor.BrandOrange)
            .Should()
            .BeTrue($"active progress dot must be {DesignColor.BrandOrange}, got {background}");
    }

    /// <summary>Summary: Stepper value inputs must use Baloo display typography, not body Nunito.</summary>
    [Fact]
    public async Task Wizard_StepperValue_UsesBalooDisplayFont()
    {
        await using var design = await WizardDesignPage.OpenAsync(host.Fixture);
        var input = design.DisplaySample;
        await Assertions.Expect(input).ToBeVisibleAsync();

        var font = await design.StyleAsync(input, "font-family");
        var radius = DesignColor.ParseCssPx(
            await design.StyleAsync(input, "border-top-left-radius")
        );

        font.Should().ContainEquivalentOf("Baloo");
        radius.Should().BeGreaterThanOrEqualTo(8);
    }

    /// <summary>Summary: Primary CTA height ≥48px and progress-dot hit target ≥40px (chips/steppers covered by Ux).</summary>
    [Fact]
    public async Task Wizard_PrimaryCtaAndProgressDots_MeetBrandTouchSizes()
    {
        await using var design = await WizardDesignPage.OpenAsync(host.Fixture);

        var ctaBox = await design.PrimaryForwardButton.BoundingBoxAsync();
        ctaBox.Should().NotBeNull();
        ctaBox!.Height.Should().BeGreaterThanOrEqualTo(48);

        var dotBox = await design.ActiveProgressDot.BoundingBoxAsync();
        dotBox.Should().NotBeNull();
        Math.Min(dotBox!.Height, dotBox.Width).Should().BeGreaterThanOrEqualTo(40);
    }

    /// <summary>Summary: Filter bottom sheet must use rounded top (~24px), matching PZ sheet language.</summary>
    [Fact]
    public async Task Wizard_FilterBottomSheet_HasRoundedTopCorners()
    {
        await using var design = await WizardDesignPage.OpenAsync(host.Fixture);
        await design.Wizard.ClickNextAsync();
        await design.Wizard.ClickNextAsync();
        await design.Wizard.OpenAddFilterSheetAsync();

        var sheet = design.BottomSheet;
        await Assertions.Expect(sheet).ToBeVisibleAsync();
        var topLeft = DesignColor.ParseCssPx(
            await design.StyleAsync(sheet, "border-top-left-radius")
        );
        var topRight = DesignColor.ParseCssPx(
            await design.StyleAsync(sheet, "border-top-right-radius")
        );

        topLeft.Should().BeGreaterThanOrEqualTo(22);
        topRight.Should().BeGreaterThanOrEqualTo(22);
    }

    /// <summary>Summary: Primary CTA must be pill rounded — not a sharp Material rectangle.</summary>
    [Fact]
    public async Task Wizard_PrimaryCta_UsesPillRadius()
    {
        await using var design = await WizardDesignPage.OpenAsync(host.Fixture);
        var box = await design.PrimaryForwardButton.BoundingBoxAsync();
        box.Should().NotBeNull();
        var radius = DesignColor.ParseCssPx(
            await design.StyleAsync(design.PrimaryForwardButton, "border-top-left-radius")
        );
        radius.Should().BeGreaterThanOrEqualTo(box!.Height / 2 - 1);
    }

    /// <summary>Summary: Swipe hint on the purple header must stay high-contrast (near-white, not washed out).</summary>
    [Fact]
    public async Task Wizard_SwipeHint_IsHighContrastOnHeader()
    {
        await using var design = await WizardDesignPage.OpenAsync(host.Fixture);
        var color = await design.StyleAsync(design.SwipeHint, "color");
        var opacity = await design.StyleAsync(design.SwipeHint, "opacity");

        DesignColor
            .TryParseCssRgba(color, out var rgba)
            .Should()
            .BeTrue($"expected rgba color, got {color}");
        rgba.A.Should()
            .BeGreaterThanOrEqualTo(
                0.85,
                because: "swipe hint must remain readable on the purple header"
            );
        ((rgba.R + rgba.G + rgba.B) / 3.0)
            .Should()
            .BeGreaterThanOrEqualTo(
                200,
                because: "swipe hint must stay near-white on the dark header"
            );
        if (
            double.TryParse(
                opacity,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var op
            )
        )
        {
            op.Should().BeGreaterThanOrEqualTo(0.85);
        }

        await Assertions.Expect(design.SwipeHint).ToBeVisibleAsync();
        await Assertions.Expect(design.SwipeHint).ToContainTextAsync("wischen");

        var hintBox = await design.SwipeHint.BoundingBoxAsync();
        var sheetBox = await design.StepSheet.BoundingBoxAsync();
        hintBox.Should().NotBeNull();
        sheetBox.Should().NotBeNull();
        hintBox!
            .Y.Should()
            .BeLessThan(
                sheetBox!.Y - 1,
                because: "swipe hint must sit fully above the white step sheet, not under its overlap"
            );
        (hintBox.Y + hintBox.Height)
            .Should()
            .BeLessThanOrEqualTo(
                sheetBox.Y + 1,
                because: "swipe hint must not be clipped by the step sheet overlap"
            );
    }
}
