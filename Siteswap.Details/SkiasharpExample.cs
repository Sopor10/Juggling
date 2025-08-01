using Siteswap.Details.CausalDiagram;
using SkiaSharp;

namespace Siteswap.Details;

public class SkiasharpExample
{
    public void Test(SKCanvas canvas)
    {
        var causalDiagram = new CausalDiagramGenerator().Generate(
            new Siteswap(5, 3, 1),
            new CyclicArray<Hand>(
                new List<Hand>
                {
                    new("R", new Person("A")),
                    new("R", new Person("B")),
                    new("L", new Person("A")),
                    new("L", new Person("B")),
                }
            )
        );
        new CausalDiagramRenderer().Render(canvas, causalDiagram);
    }
}
