using System.Diagnostics;
using DotNetGraph.Extensions;
using FluentAssertions;
using MoreLinq;
using Siteswap.Details;
using Siteswap.Details.StateDiagram;
using Siteswap.Details.StateDiagram.Graph;
using Siteswaps.Visualization;

namespace Siteswaps.Test;

public class SiteswapGraphTest
{
    [Test]
    public void Advance_State_Forward()
    {
        var state = State.GroundState(3, 5);

        state.Advance()
            .Should()
            .Be(State.GroundState(2, 5));
    }
    
    [Test]
    public void State_Is_Presented_Corrected()
    {
        var state = State.GroundState(3, 5);

        state.StateRepresentation()
            .Should()
            .Be("11100");
    }
    
    [Test]
    public void Throw_4()
    {
        var state = State.GroundState(3, 5);

        state.Throw(4)
            .Should()
            .Be(State.GroundState(4, 5));
    }


    [Test]
    public void Siteswap_531_Is_Ground_State()
    {
        CalculateState(5, 3, 1)
            .Should()
            .Be(State.GroundState(3,5));
    }

    
    [Test]
    public void Siteswap_414_State_Is_Correct()
    {
        CalculateState(4,1,4)
            .StateRepresentation()
            .Should()
            .Be("1101");
    }

    [Test]
    public void METHOD()
    {
        var stategraph = CalculateGraph(5, 3, 1);
        var dotgraph = new GraphFactory().Create(stategraph).Compile(true);
        File.WriteAllText("dotfile.txt", dotgraph);
    }
    
    private StateGraph CalculateGraph(params int[] siteswap)
    {
        var cyclicArray = siteswap.ToCyclicArray();

        var states = new List<State>();
        for (var i = 0; i < siteswap.Length; i++)
        {
            var rotate = cyclicArray.Rotate(i);
            var calculateState = CalculateState(rotate.EnumerateValues(1).ToArray());
            states.Add(calculateState);
        }

        var mappedStates = new HashSet<Siteswap.Details.StateDiagram.State>(states.Select(Map).ToList());

        var cyclicArrayStates = new CyclicArray<Siteswap.Details.StateDiagram.State>(mappedStates);
        var edges = new HashSet<Edge<Siteswap.Details.StateDiagram.State, int>>();
        for (int i = 0; i < cyclicArrayStates.Length; i++)
        {
            edges.Add(new Edge<Siteswap.Details.StateDiagram.State, int>(cyclicArrayStates[i], cyclicArrayStates[i + 1],
                siteswap[i]));
        }
        
        
        
        return new StateGraph(new Graph<Siteswap.Details.StateDiagram.State, int>(mappedStates, edges));

    }

    private Siteswap.Details.StateDiagram.State Map(State state)
    {
        return new(state.StateRepresentation()
            .Select(c => c switch
            {
                '0' => false,
                '1' => true
            }));
    }


    private State CalculateState(params int[] siteswap)
    {
        var stable = false;

        var state = State.Empty(siteswap.Max());

        while (stable is false)
        {
            var previousState = state;
            foreach (var value in siteswap)
            {
                state = state.Advance().Throw(value);
                
            }

            if (state == previousState)
            {
                stable = true;
            }
        }

        return state;
    }

    [DebuggerDisplay("{StateRepresentation()}")]
    private record State(uint Value, int Length)
    {
        public static State Empty(int length)
        {
            return new(0, length);
        }

        public static State GroundState(int numberOfBalls, int length)
        {
            if (length < numberOfBalls) throw new ArgumentException();

            var mask = 0xffffffff;
            mask >>= 32 - numberOfBalls;
            mask <<= 0;
            return new State(mask, length);
        }

        public string StateRepresentation()
        {
            return string.Concat(Convert.ToString(Value, 2).Reverse().ToArray()) + string.Concat("0".Repeat(Length - Convert.ToString(Value, 2).Length));
        }

        public override string ToString()
        {
            return StateRepresentation();
        }

        public State Advance()
        {
            var advance = this with
            {
                Value = Value >> 1
            };
            return advance;
        }

        public State Throw(int i)
        {
            var state = this with
            {
                Value = Value | (uint)(1 << (i - 1))
            };
            return state;
        }
    }
}
