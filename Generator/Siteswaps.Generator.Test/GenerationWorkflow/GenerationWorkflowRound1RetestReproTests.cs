using System.Text.RegularExpressions;
using FluentAssertions;
using Siteswaps.Generator.Components.WizardPage.Filters;

namespace Siteswaps.Generator.Test.GenerationWorkflow;

/// <summary>
/// Round-1 retest repros (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class GenerationWorkflowRound1RetestReproTests
{
    /// <summary>
    /// Finding (Critical): Host passed MaxHeight= while FilterBottomSheet no longer declares it.
    /// Soll: host markup must not bind a stale height parameter name.
    /// </summary>
    [Test]
    public void ConfiguredGenerationWorkflow_Binds_FilterBottomSheet_Height_With_Declared_Parameter_Name()
    {
        var hostRazor = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine("Components", "GenerationWorkflow", "ConfiguredGenerationWorkflow.razor")
        );

        var sheetBlock = ExtractFilterBottomSheetMarkup(hostRazor);
        sheetBlock
            .Should()
            .NotBeNullOrWhiteSpace("ConfiguredGenerationWorkflow must host FilterBottomSheet");

        sheetBlock!
            .Should()
            .NotMatch(
                """\bMaxHeight\s*=""",
                "FilterBottomSheet no longer declares MaxHeight; a stale bind crashes at runtime"
            );

        typeof(FilterBottomSheet)
            .GetProperties(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
            )
            .Select(p => p.Name)
            .Should()
            .NotContain(
                "MaxHeight",
                "height is derived from allowed throws now, not a separate bottom-sheet parameter"
            );
    }

    private static string? ExtractFilterBottomSheetMarkup(string hostRazor)
    {
        var match = Regex.Match(
            hostRazor,
            """<FilterBottomSheet\b[\s\S]*?(?:/>|</FilterBottomSheet>)""",
            RegexOptions.IgnoreCase
        );
        return match.Success ? match.Value : null;
    }
}
