using System.Collections.Immutable;

namespace Siteswap.Details.CausalDiagram;

public class CausalDiagramGenerator
{
    public CausalDiagram Generate(Siteswap siteswap, CyclicArray<Hand> hands)
    {
        var nodes = new List<Throw>();

        for (var j = 0; j < siteswap.Items.Length; j++)
        {
            for (var i = 0; i < hands.Length; i++)
            {
                var currentPos = j * hands.Length + i;
                var node = new Throw(
                    hands[currentPos],
                    siteswap.Items[currentPos],
                    j + CalculateOffset(hands.Length, currentPos).Value
                );
                nodes.Add(node);
            }
        }

        var transitions = new List<Transition>();
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var forcedThrow = i + siteswap.Items[i] - hands.Length;
            if (IsInRange(forcedThrow, nodes))
            {
                var target = nodes[forcedThrow];
                Transition transition = new(node, target);
                transitions.Add(transition);
            }
        }
        return new CausalDiagram(nodes.ToImmutableList(), transitions.ToImmutableList());
    }

    private static bool IsInRange(int index, List<Throw> nodes)
    {
        return index >= 0 && index < nodes.Count;
    }

    private Offset CalculateOffset(int numberOfHands, int currentPosition)
    {
        return new Offset(1.0m * (currentPosition % numberOfHands) / numberOfHands);
    }
}
