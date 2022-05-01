using System.Collections.Generic;
using NUnit.Framework;
using Siteswap.Details;
using Siteswap.Details.CausalDiagram;
using SkiaSharp;

namespace Siteswaps.Test.CausalDiagram;

public class CausalDiagramRendererTests
{
    [Test]
    public void METHOD()
    {
        var bitmap = new SKBitmap(500, 500);
        
        using var canvas = new SKCanvas(bitmap);
        
        var causalDiagram = new CausalDiagramGenerator().Generate(new Siteswap.Details.CausalDiagram.Siteswap(new CyclicArray<int>(
            new List<int>
            {
                5, 3, 1
            })), new CyclicArray<Hand>(new List<Hand>()
        {
            new Hand("R", new Person("A")),
            new Hand("R", new Person("B")),
            new Hand("L", new Person("A")),
            new Hand("L", new Person("B")),
        }));
        new CausalDiagramRenderer().Render(canvas, causalDiagram);
        var image = SKImage.FromBitmap(bitmap);
        var data = image.Encode();
        
        
    }
}