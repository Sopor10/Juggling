using DotNetGraph.Extensions;
using FluentAssertions;
using Siteswap.Details;
using Siteswap.Details.StateDiagram;
using Siteswap.Details.StateDiagram.Graph;
using Siteswaps.Visualization;

namespace Siteswaps.Test;

public class SiteswapGraphTest : VerifyBase
{
    public SiteswapGraphTest() : base()
    {
        
    }
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
    public async Task Create_Graph_For_Siteswap_531()
    {
        await Verify(CalculateGraph(5, 3, 1).Graph).AddExtraSettings(_=>
            {
                _.Converters.Add(new StateConverter());
                _.Converters.Add(new EdgeConverter());
                
            }
            );
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

        var mappedStates = new HashSet<State>(states.Select(Map).ToList());

        var cyclicArrayStates = new CyclicArray<State>(mappedStates);
        var edges = new HashSet<Edge<State, int>>();
        for (int i = 0; i < cyclicArrayStates.Length; i++)
        {
            edges.Add(new Edge<State, int>(cyclicArrayStates[i], cyclicArrayStates[i + 1],
                siteswap[i]));
        }
        
        
        
        return new StateGraph(new Graph<State, int>(mappedStates, edges));

    }

    private State Map(State state)
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
}

public class StateConverter :
    WriteOnlyJsonConverter<State>
{
    public override void Write(VerifyJsonWriter writer, State state) =>
        writer.WriteValue(state.StateRepresentation());
}

public class EdgeConverter :
    WriteOnlyJsonConverter<Edge<State, int>>
{
    public override void Write(VerifyJsonWriter writer, Edge<State, int> edge) =>
        writer.WriteValue(edge.N1.StateRepresentation() + " -" + edge.Data + "-> " + edge.N2.StateRepresentation());
}