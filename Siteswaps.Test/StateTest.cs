using FluentAssertions;
using Siteswap.Details.StateDiagram;
using Siteswap.Details.StateDiagram.Graph;

namespace Siteswaps.Test;

public class StateTest
{
    [Test]
    public void StateTransitionsCanBeCalculated()
    {
        var sut = new State(1, 0, 1, 1, 0);
        var transitions = sut.Transitions(5);
        transitions.Should().BeEquivalentTo(new Edge<State, int>[]
        {
            new(sut, new State(1,1,1,0,0), 1),
            new(sut, new State(0,1,1,1,0), 4),
            new(sut, new State(0,1,1,0,1), 5),
        });
    }
    [Test]
    public void State_Transition_From_Zero_Can_Be_Calculated()
    {
        var sut = new State(0, 0, 1, 1, 0);
        var transitions = sut.Transitions(5);
        transitions.Should().BeEquivalentTo(new Edge<State, int>[]
        {
            new(sut, new State(0,1,1,0,0), 0),
        });
    }
    
    [Test]
    public void State_Transition_From_Ground_State()
    {
        var sut = new State(1, 1, 1, 0, 0);
        var transitions = sut.Transitions(5);

        transitions.Should().HaveCount(3);
        transitions.Should().BeEquivalentTo(new Edge<State, int>[]
        {
            new(sut, new State(1,1,1,0,0), 3),
            new(sut, new State(1,1,0,1,0), 4),
            new(sut, new State(1,1,0,0,1), 5),
        });
    }
}