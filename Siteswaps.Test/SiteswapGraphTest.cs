using FluentAssertions;
using Siteswap.Details.StateDiagram;
using Siteswap.Details.StateDiagram.Graph;

namespace Siteswaps.Test;

public class SiteswapGraphTest : VerifyBase
{
    public SiteswapGraphTest()
        : base() { }

    [Test]
    public void Advance_State_Forward()
    {
        var state = State.GroundState(3);

        state.Advance().Should().Be(State.GroundState(2));
    }

    [Test]
    public void State_Is_Presented_Corrected()
    {
        var state = State.GroundState(3);

        state.StateRepresentation().Should().Be("111");
    }

    [Test]
    public void Throw_4()
    {
        var state = State.GroundState(3);

        state.Throw(4).Should().Be(State.GroundState(4));
    }

    [Test]
    public void Siteswap_531_Is_Ground_State()
    {
        var siteswap = new[] { 5, 3, 1 };
        StateGenerator.CalculateState(siteswap).Should().Be(State.GroundState(3));
    }

    [Test]
    public void Siteswap_414_State_Is_Correct()
    {
        var siteswap = new[] { 4, 1, 4 };
        StateGenerator.CalculateState(siteswap).StateRepresentation().Should().Be("1101");
    }

    [Test]
    public async Task Create_Graph_For_Siteswap_531()
    {
        var siteswap = new Siteswap.Details.Siteswap(5, 3, 1);
        await Verify(StateGraphFromSiteswapGenerator.CalculateGraph(siteswap).Graph)
            .AddExtraSettings(_ =>
            {
                _.Converters.Add(new StateConverter());
                _.Converters.Add(new EdgeConverter());
            });
    }

    [Test]
    public async Task Create_Graph_For_Siteswap_531441423()
    {
        var siteswap = new Siteswap.Details.Siteswap(5, 3, 1, 4, 4, 1, 4, 2, 3);
        await Verify(StateGraphFromSiteswapGenerator.CalculateGraph(siteswap).Graph)
            .AddExtraSettings(_ =>
            {
                _.Converters.Add(new StateConverter());
                _.Converters.Add(new EdgeConverter());
            });
    }
}

public class StateConverter : WriteOnlyJsonConverter<State>
{
    public override void Write(VerifyJsonWriter writer, State state)
    {
        writer.WriteValue(state.StateRepresentation());
    }
}

public class EdgeConverter : WriteOnlyJsonConverter<Edge<State, int>>
{
    public override void Write(VerifyJsonWriter writer, Edge<State, int> edge)
    {
        writer.WriteValue(
            edge.N1.StateRepresentation() + " -" + edge.Data + "-> " + edge.N2.StateRepresentation()
        );
    }
}
