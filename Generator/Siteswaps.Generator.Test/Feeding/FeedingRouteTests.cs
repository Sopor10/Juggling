using FluentAssertions;
using Siteswaps.Generator.Components.Feeding;

namespace Siteswaps.Generator.Test.Feeding;

/// <summary>
/// Feeding route entry: app-relative URL and host page wiring (PathBase-safe).
/// </summary>
[TestFixture]
public class FeedingRouteTests
{
    [Test]
    public void FeedingHref_Is_App_Relative_With_Siteswap_Query()
    {
        FeedingRouteLinks.FromNotation("756").Should().Be("feeding?s=756");
        FeedingRouteLinks.FromNotation("756").Should().NotStartWith("/");
        FeedingRouteLinks.FromNotation("aB").Should().Be("feeding?s=aB");
    }

    [Test]
    public void FeedingHref_Escapes_Notation_For_Query()
    {
        FeedingRouteLinks.FromNotation("7 5").Should().Be("feeding?s=7%205");
    }

    [Test]
    public void FeedingPage_Declares_Feeding_Route_And_Query_Parameter()
    {
        var razor = ReadFeedingPageRazor();
        var code = ReadFeedingPageCodeBehind();

        razor.Should().Contain("@page \"/feeding\"");
        code.Should().MatchRegex("""SupplyParameterFromQuery\(Name\s*=\s*"s"\)""");
    }

    [Test]
    public void FeedingPage_Orchestrates_NormalFeedSession_And_ConfiguredGenerationWorkflow()
    {
        var razor = ReadFeedingPageRazor();
        var code = ReadFeedingPageCodeBehind();
        var combined = razor + code;

        combined.Should().Contain("NormalFeedSession");
        combined.Should().Contain("ConfiguredGenerationWorkflow");
        combined.Should().Contain("ToGenerationWorkflowConfig");
    }

    [Test]
    public void FeedingPage_Exists_As_Generator_Component()
    {
        var type = typeof(Siteswaps.Generator.Components.Feeding.FeedingPage);
        type.Should().NotBeNull();
    }

    [Test]
    public void FeedingPage_Uses_TryCreate_And_Shows_Load_Error_Markup()
    {
        var razor = ReadFeedingPageRazor();
        var code = ReadFeedingPageCodeBehind();

        code.Should().Contain("Siteswap.TryCreate");
        razor.Should().Contain("role=\"alert\"");
        razor.Should().Contain("aria-pressed");
        razor.Should().Contain("aria-live");
    }

    private static string ReadFeedingPageRazor() =>
        ReadGeneratorSource(Path.Combine("Components", "Feeding", "FeedingPage.razor"));

    private static string ReadFeedingPageCodeBehind() =>
        ReadGeneratorSource(Path.Combine("Components", "Feeding", "FeedingPage.razor.cs"));

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
