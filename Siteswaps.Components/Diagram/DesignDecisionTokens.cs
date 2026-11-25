using Microsoft.JSInterop;

namespace Siteswaps.Components.Diagram;

internal sealed record DiagramDesignDecisions(
    string TextStrong,
    string BrandStrong,
    string BrandAccent,
    string BorderSubtle,
    string ActionPrimary,
    string White,
    string FontBody
);

internal static class DesignDecisionTokens
{
    private static readonly string[] DiagramTokenNames =
    [
        "--decision-color-text-strong",
        "--decision-color-brand-strong",
        "--decision-color-brand-accent",
        "--decision-color-border-subtle",
        "--decision-color-action-primary",
        "--decision-color-white",
        "--decision-font-body",
    ];

    public static async Task<DiagramDesignDecisions> ReadDiagramAsync(IJSRuntime js)
    {
        var module = await js.InvokeAsync<IJSObjectReference>(
            "import",
            "./_content/Siteswaps.Components/designTokens.js"
        );

        try
        {
            var values = await module.InvokeAsync<string[]>(
                "readDecisionTokens",
                DiagramTokenNames
            );
            if (values.Length != DiagramTokenNames.Length || values.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidOperationException("A required design decision token is missing.");
            }

            return new DiagramDesignDecisions(
                values[0],
                values[1],
                values[2],
                values[3],
                values[4],
                values[5],
                values[6]
            );
        }
        finally
        {
            await module.DisposeAsync();
        }
    }
}
