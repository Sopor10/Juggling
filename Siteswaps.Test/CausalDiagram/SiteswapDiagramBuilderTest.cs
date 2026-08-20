using FluentAssertions;
using Siteswap.Details;
using Siteswap.Details.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public class SiteswapDiagramBuilderTest
{
    [Test]
    public void PassingHandOrder_TwoJugglers_IsArBrAlBl()
    {
        var hands = PassingHandOrder.Create(2);

        hands.Length.Should().Be(4);
        hands[0].Should().Be(new Hand("R", new Person("A")));
        hands[1].Should().Be(new Hand("R", new Person("B")));
        hands[2].Should().Be(new Hand("L", new Person("A")));
        hands[3].Should().Be(new Hand("L", new Person("B")));
    }

    [Test]
    public void Build_757245_CausalSubtractsNumberOfHands_LadderUsesFullHeight()
    {
        Siteswap.Details.Siteswap.TryCreate("757245", out var siteswap).Should().BeTrue();
        var diagram = SiteswapDiagramBuilder.Build(siteswap!, numberOfJugglers: 2);

        diagram.Hands.Count.Should().Be(4);
        diagram.TimeStretchFactor.Should().Be(2m);
        // LCM(orbit sums 12 & 18, limbs 4) = 36 — mirrors passist toJif repetition.period
        diagram.Throws.Count.Should().Be(36);

        var firstCausal = diagram.CausalArrows[0];
        firstCausal.From.Height.Should().Be(7);
        firstCausal.Step.Should().Be(3); // 7 - 4 hands
        firstCausal.To.Time.Should().Be(3);

        var firstLadder = diagram.LadderArrows[0];
        firstLadder.From.Height.Should().Be(7);
        firstLadder.Step.Should().Be(7);
        firstLadder.To.Time.Should().Be(7);
    }
}
