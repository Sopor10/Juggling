
using FluentAssertions;
using Siteswap.Details.StateDiagram;

namespace Siteswaps.Test.StateDiagram;

public class CreateTransitionsTest
{
    
    [Test]
    public void Create_Specific_Transition_Length_1()
    {
        var from = new Siteswap.Details.Siteswap(5, 3, 1);
        var to = new Siteswap.Details.Siteswap(4,1,4);
        var length = 1;

        var transitions = TransitionGenerator.CreateTransitions(from, to, length);

        transitions.Should().ContainSingle().Which.Throws.Should().ContainSingle().Which.Should().Be(4);
    }
    
    [Test]
    public void Create_Specific_Transition_Length_2()
    {
        var from = new Siteswap.Details.Siteswap(5, 3, 1);
        var to = new Siteswap.Details.Siteswap(4,1,4);
        var length = 2;

        var transitions = TransitionGenerator.CreateTransitions(from, to, length);

        transitions.Where(x => x.Throws.Length == 2).Should().HaveCount(2);
    }
}