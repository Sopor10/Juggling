using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Siteswaps.Generator.Components.WizardPage.Filters;

namespace Siteswaps.Generator.Test.GenerationWorkflow;

/// <summary>
/// Round-1 retest repros (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class GenerationWorkflowRound1RetestReproTests
{
    /// <summary>
    /// Finding (Critical): Host passes MaxHeight= but FilterBottomSheet declares MaxThrowHeight
    /// → Blazor crash when Feeding opens Generate (ConfiguredGenerationWorkflow renders the sheet).
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

        var declaredHeightParams = typeof(FilterBottomSheet)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.IsDefined(typeof(ParameterAttribute), inherit: true))
            .Select(p => p.Name)
            .Where(n => n.Contains("Height", StringComparison.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.Ordinal);

        declaredHeightParams
            .Should()
            .NotBeEmpty("FilterBottomSheet must declare a height Parameter");

        var boundHeight = Regex.Match(
            sheetBlock!,
            """\b(Max(?:Throw)?Height)\s*=""",
            RegexOptions.IgnoreCase
        );
        boundHeight
            .Success.Should()
            .BeTrue("FilterBottomSheet host markup must bind a height parameter");

        var boundName = boundHeight.Groups[1].Value;
        declaredHeightParams
            .Should()
            .Contain(
                boundName,
                "Host must use the FilterBottomSheet parameter name (wrong name → runtime crash)"
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
