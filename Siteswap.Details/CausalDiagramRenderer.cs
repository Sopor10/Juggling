﻿using Siteswap.Details.CausalDiagram;
using SkiaSharp;

namespace Siteswap.Details;

public class CausalDiagramRenderer
{
    private readonly SKPaint circlePaint = new()
    {
        Color = new SKColor(255, 255, 255, 255),
        StrokeWidth = 1,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
    };

    private readonly SKPaint namePaint = new()
    {
        Color = new SKColor(255, 0, 0, 255),
        StrokeWidth = 1,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
    };

    private readonly SKPaint handPaint = new()
    {
        Color = new SKColor(255, 100, 175, 255),
        StrokeWidth = 1,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
    };

    private readonly SKPaint transitionPaint = new()
    {
        Color = new SKColor(100, 255, 100, 255),
        StrokeWidth = 2,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
    };

    public void Render(SKCanvas canvas, CausalDiagram.CausalDiagram diagram)
    {
        var persons = diagram.Nodes.GroupBy(x => x.Hand.Person).ToList();
        DrawNodes(canvas, diagram, persons);
        DrawHandNames(canvas, diagram, persons);
        DrawPersonNames(canvas, persons);
        DrawTransitions(canvas, diagram, persons);
    }

    private void DrawTransitions(
        SKCanvas canvas,
        CausalDiagram.CausalDiagram diagram,
        List<IGrouping<Person, Node>> persons
    )
    {
        var positions = CalculateNodePositions(diagram, persons);
        foreach (var (start, end) in diagram.Transitions)
        {
            var endPoint = positions[end];
            var startPoint = positions[start];
            canvas.DrawLine(startPoint, endPoint, transitionPaint);
        }
    }

    private void DrawPersonNames(SKCanvas canvas, IEnumerable<IGrouping<Person, Node>> persons)
    {
        var height = 100;
        foreach (var person in persons)
        {
            canvas.DrawText(
                person.Key.Name,
                new SKPoint(50, height),
                SKTextAlign.Center,
                new SKFont(),
                namePaint
            );
            height += 50;
        }
    }

    private void DrawNodes(
        SKCanvas canvas,
        CausalDiagram.CausalDiagram diagram,
        IEnumerable<IGrouping<Person, Node>> persons
    )
    {
        var points = CalculateNodePositions(diagram, persons);

        foreach (var (node, skPoint) in points)
        {
            canvas.DrawCircle(skPoint, 10, circlePaint);
            canvas.DrawText(
                node.Height.ToString(),
                skPoint,
                SKTextAlign.Center,
                new SKFont(),
                circlePaint
            );
        }
    }

    private Dictionary<Node, SKPoint> CalculateNodePositions(
        CausalDiagram.CausalDiagram diagram,
        IEnumerable<IGrouping<Person, Node>> persons
    )
    {
        var points = new Dictionary<Node, SKPoint>();

        var height = 100;
        foreach (var person in persons)
        {
            foreach (var node in person)
            {
                var skPoint = ToSkPoint(node, diagram, height);
                points.Add(node, skPoint);
            }

            height += 50;
        }

        return points;
    }

    private void DrawHandNames(
        SKCanvas canvas,
        CausalDiagram.CausalDiagram diagram,
        IEnumerable<IGrouping<Person, Node>> persons
    )
    {
        var points = new List<(Node, SKPoint)>();
        var height = 100;
        foreach (var person in persons)
        {
            foreach (var node in person)
            {
                var point = new SKPoint(ToSkPoint(node, diagram, height).X, 50);
                points.Add((node, point));
            }

            height += 50;
        }

        foreach (var (node, point) in points)
        {
            canvas.DrawText(node.Hand.Name, point, SKTextAlign.Center, new SKFont(), handPaint);
        }
    }

    private static SKPoint ToSkPoint(
        Node node,
        CausalDiagram.CausalDiagram causalDiagram,
        int height
    )
    {
        return new SKPoint((float)(node.Time / causalDiagram.MaxTime * 300 + 100), height);
    }
}
