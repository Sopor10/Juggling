using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;
using Siteswaps.Generator.Components.Feeding;

namespace Siteswaps.Generator.Test.Feeding;

/// <summary>
/// Round-1 retest repros for Feeding (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class FeedingRound1RetestReproTests
{
    /// <summary>
    /// Finding (High): /feeding without ?s= — OnParametersSet early-returns when
    /// _loadedNotation and SiteswapNotation are both null → blank page, no alert.
    /// </summary>
    [Test]
    public void FeedingPage_Without_Siteswap_Query_Sets_Load_Error()
    {
        var page = new FeedingPage();
        InjectLocalizer(page);

        page.SiteswapNotation.Should().BeNull();
        InvokeOnParametersSet(page);

        var loadError = GetPrivateField<object?>(page, "_loadError");
        loadError
            .Should()
            .NotBeNull(
                "missing ?s= must set a load error (TryLoadSession), not skip via null==null early-return"
            );
    }

    /// <summary>
    /// Finding (Critical/High): DualRangeSlider labels use .wizard-sr-only, but that rule
    /// lives only under .wizard-page ::deep — Feeding leaves labels visible on the track.
    /// </summary>
    [Test]
    public void Feeding_Or_DualRangeSlider_Styles_Hide_WizardSrOnly_Labels()
    {
        var feedingCss = ReadGeneratorSource(
            Path.Combine("Components", "Feeding", "FeedingPage.razor.css")
        );
        var dualRangeCss = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "Controls", "DualRangeSlider.razor.css")
        );

        var hidesSrOnly =
            ContainsSrOnlyHideRule(feedingCss) || ContainsSrOnlyHideRule(dualRangeCss);

        hidesSrOnly
            .Should()
            .BeTrue(
                "Feeding scoped CSS or DualRangeSlider CSS must hide .wizard-sr-only (WizardPage isolation does not apply on /feeding)"
            );
    }

    private static bool ContainsSrOnlyHideRule(string css) =>
        css.Contains("wizard-sr-only", StringComparison.Ordinal);

    private static void InjectLocalizer(FeedingPage page)
    {
        var localizer = new Mock<IStringLocalizer<FeedingPage>>();
        localizer
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        typeof(FeedingPage)
            .GetProperty("L", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(page, localizer.Object);
    }

    private static void InvokeOnParametersSet(FeedingPage page)
    {
        var method = typeof(FeedingPage).GetMethod(
            "OnParametersSet",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        method.Should().NotBeNull();
        method!.Invoke(page, null);
    }

    private static T GetPrivateField<T>(object instance, string name)
    {
        var field = instance
            .GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull($"expected private field {name}");
        return (T)field!.GetValue(instance)!;
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
