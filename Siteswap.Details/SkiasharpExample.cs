using System;
using System.Collections.Generic;
using Siteswap.Details.CausalDiagram;
using SkiaSharp;

namespace Siteswap.Details;

public class SkiasharpExample
{
    private SKImageInfo _imageInfo = new(
        500,
        500,
        SKColorType.Rgba8888,
        SKAlphaType.Premul);

    public void Test(SKCanvas canvas)
    {
        var causalDiagram = new CausalDiagramGenerator().Generate(new CausalDiagram.Siteswap(new CyclicArray<int>(
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
    }
}

public class CausalDiagramRenderer
{
    private SKImageInfo _imageInfo = new(
        500,
        500,
        SKColorType.Rgba8888,
        SKAlphaType.Premul);

    public void Render(SKCanvas canvas, CausalDiagram.CausalDiagram diagram)
    {
        var rand = new Random();

        var lineColor = new SKColor(
            (byte)rand.Next(255),
            (byte)rand.Next(255),
            (byte)rand.Next(255),
            (byte)rand.Next(255));

        var linePaint = new SKPaint
        {
            Color = lineColor,
            StrokeWidth = 5,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };
        foreach (var node in diagram.Nodes)
        {
            var skPoint = ToSkPoint(node, diagram);
            canvas.DrawCircle(skPoint, 10, linePaint);
        }
    }

    private static SKPoint ToSkPoint(Node node, CausalDiagram.CausalDiagram causalDiagram)
    {
        
        return new SKPoint((float)(node.Time/causalDiagram.MaxTime * 500), 0);
    }
}