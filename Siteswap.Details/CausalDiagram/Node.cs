using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Siteswap.Details.CausalDiagram;

public record Offset(Decimal Value);
public record Person(string Name);

[DebuggerDisplay("{Person.Name}_{Name}")]
public record Hand(string Name, Person Person);
public record Node(Hand Hand, int Height, decimal Time);
public record Transition(Node Start, Node End);

public record Siteswap(CyclicArray<int> Values)
{
    public Siteswap(params int[] values):this(new CyclicArray<int>(values))
    {
    }
}

public record CausalDiagram(ImmutableList<Node> Nodes, ImmutableList<Transition> Transitions)
{
    public decimal MaxTime => Nodes.Max(x => x.Time);
}

public class CausalDiagramGenerator
{
    public CausalDiagram Generate(Siteswap siteswap, CyclicArray<Hand> hands)
    {
        var nodes = new List<Node>();

        for (var j = 0; j < siteswap.Values.Length; j++)
        {
            for (var i = 0; i < hands.Length; i++)
            {
                var currentPos = j * hands.Length + i;
                var node = new Node(hands[currentPos], siteswap.Values[currentPos], j + CalculateOffset(hands.Length, currentPos).Value);
                nodes.Add(node);
            }
        }

        var transitions = new List<Transition>();
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var forcedThrow = i + siteswap.Values[i] - hands.Length;
            if (IsInRange(forcedThrow, nodes))
            {
                var target = nodes[forcedThrow];
                Transition transition = new(node, target);
                transitions.Add(transition);
            }
        }            
        return new CausalDiagram(nodes.ToImmutableList(), transitions.ToImmutableList());
    }

    private static bool IsInRange(int index, List<Node> nodes)
    {
        return index >=0 && index < nodes.Count;
    }

    private Offset CalculateOffset(int numberOfHands, int currentPosition)
    {
        return new Offset(1.0m * (currentPosition % numberOfHands) / numberOfHands);
    }
}