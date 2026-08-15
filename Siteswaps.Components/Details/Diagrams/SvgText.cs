using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Siteswaps.Components.Details.Diagrams;

/// <summary>
/// Razor reserves the HTML <c>text</c> tag, so SVG text nodes are rendered via the render tree.
/// </summary>
public sealed class SvgText : ComponentBase
{
    [Parameter]
    public string? X { get; set; }

    [Parameter]
    public string? Y { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public string? TextAnchor { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "text");
        if (X is not null)
        {
            builder.AddAttribute(1, "x", X);
        }

        if (Y is not null)
        {
            builder.AddAttribute(2, "y", Y);
        }

        if (CssClass is not null)
        {
            builder.AddAttribute(3, "class", CssClass);
        }

        if (TextAnchor is not null)
        {
            builder.AddAttribute(4, "text-anchor", TextAnchor);
        }

        if (AdditionalAttributes is not null)
        {
            builder.AddMultipleAttributes(5, AdditionalAttributes);
        }

        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }
}
