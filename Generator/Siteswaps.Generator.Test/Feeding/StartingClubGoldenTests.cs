using FluentAssertions;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.Feeding;

[TestFixture]
public class StartingClubGoldenTests
{
    [Test]
    public void Starting_Clubs_For_78627_Are_J0_2_2_And_J1_1_1()
    {
        var heights = new[] { 7, 8, 6, 2, 7 };

        StartingClubDistribution.ForJuggler(heights, juggler: 0).Should().Be(new ClubHands(2, 2));
        StartingClubDistribution.ForJuggler(heights, juggler: 1).Should().Be(new ClubHands(1, 1));
    }

    [Test]
    public void Session_StartingClubs_For_A_Uses_Feeder_Without_Prior_Select()
    {
        // Findings #3/#17/#21/#44/#49: A/feeder start clubs should be first-class.
        var session = NormalFeedSession.FromFeederSiteswap(
            Siteswap.CreateFromCorrect(7, 8, 6, 2, 7)
        );

        var act = () => session.StartingClubs("A");

        act.Should().NotThrow();
        session.StartingClubs("A").Should().Be(new ClubHands(2, 2));
    }

    [Test]
    public void Session_StartingClubs_For_B1_With_Selected_Matching_Interface_Is_Juggler1_Hands()
    {
        var session = NormalFeedSession.FromFeederSiteswap(
            Siteswap.CreateFromCorrect(7, 8, 6, 2, 7)
        );
        session.AssignPass(0, "B1");
        session.AssignPass(4, "B2");
        // Landing interface B1 = S,S,P,S,S (not feeder 78627 throw pattern).
        var selected = Siteswap.CreateFromCorrect(1, 2, 0, 2, 0);
        session.SelectSiteswap("B1", selected);

        session
            .StartingClubs("B1")
            .Should()
            .Be(StartingClubDistribution.ForJuggler(selected.Items, juggler: 1));
    }
}
