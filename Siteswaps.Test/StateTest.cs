using FluentAssertions;
using NUnit.Framework;
using Siteswaps.StateDiagram;
using Siteswaps.StateDiagram.Graph;

namespace Siteswaps.Test;

public class StateTest
{
    [Test]
    public void StateTransitionsCanBeCalculated()
    {
        var sut = new State(1, 0, 1, 1, 0);
        var transitions = sut.Transitions();
        transitions.Should().BeEquivalentTo(new Edge<State, int>[]
        {
            new(sut, new State(1,1,1,0,0), 1),
            new(sut, new State(0,1,1,1,0), 4),
        });
    }
    [Test]
    public void State_Transition_From_Zero_Can_Be_Calculated()
    {
        var sut = new State(0, 0, 1, 1, 0);
        var transitions = sut.Transitions();
        transitions.Should().BeEquivalentTo(new Edge<State, int>[]
        {
            new(sut, new State(0,1,1,0,0), 0),
        });
    }
}