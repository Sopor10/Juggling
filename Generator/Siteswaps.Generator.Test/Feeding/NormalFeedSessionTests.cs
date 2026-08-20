using FluentAssertions;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.Feeding;

[TestFixture]
public class NormalFeedTopologyTests
{
    [Test]
    public void NormalFeed_Defines_A_As_Feeder_With_B1_And_B2_On_Second_Time_Layer()
    {
        var topology = NormalFeed.Create();

        topology.A.Name.Should().Be("A");
        topology.A.TimeZone.Should().Be(0);
        topology.A.PassingPartners.Should().Equal("B1", "B2");

        topology.B1.Name.Should().Be("B1");
        topology.B1.TimeZone.Should().Be(1);
        topology.B1.PassingPartners.Should().Equal("A");

        topology.B2.Name.Should().Be("B2");
        topology.B2.TimeZone.Should().Be(1);
        topology.B2.PassingPartners.Should().Equal("A");
    }
}

[TestFixture]
public class PassAssignmentTests
{
    [Test]
    public void Incomplete_Pass_Assignments_Prevent_Generation()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));

        session.AssignPass(0, "B1");

        session.ArePassAssignmentsComplete.Should().BeFalse();
        session.CanGenerate.Should().BeFalse();
    }

    [Test]
    public void Complete_Pass_Assignments_Allow_Generation()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));

        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");
        // Explicit clubs: default 0–0 must not count as generation-ready (see Clubs_Default_Zero_Zero…).
        session.ClubsB1 = new Between { MinNumber = 3, MaxNumber = 3 };
        session.ClubsB2 = new Between { MinNumber = 3, MaxNumber = 3 };

        session.ArePassAssignmentsComplete.Should().BeTrue();
        session.CanGenerate.Should().BeTrue();
    }

    [Test]
    public void Assigning_A_Partner_On_A_Self_Throw_Is_Rejected()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));

        var act = () => session.AssignPass(2, "B1");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Self_Throws_Are_Exposed_As_Self_And_Passes_Need_Partners()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));

        session.ThrowKinds.Should().Equal(PassOrSelf.Pass, PassOrSelf.Pass, PassOrSelf.Self);
        session.PassBeatIndexes.Should().Equal(0, 1);
    }
}

[TestFixture]
public class InterfaceTranslationTests
{
    [Test]
    public void Builds_Pass_Self_Interface_For_B1_And_B2_From_Complete_Assignments()
    {
        var session = NormalFeedSession.FromFeederSiteswap(
            Siteswap.CreateFromCorrect(7, 8, 6, 2, 7)
        );
        session.AssignPass(0, "B1");
        session.AssignPass(4, "B2");

        session
            .InterfaceFor("B1")
            .Should()
            .Equal(Throw.AnySelf, Throw.AnySelf, Throw.AnyPass, Throw.AnySelf, Throw.AnySelf);

        session
            .InterfaceFor("B2")
            .Should()
            .Equal(Throw.AnySelf, Throw.AnyPass, Throw.AnySelf, Throw.AnySelf, Throw.AnySelf);
    }

    [Test]
    public void InterfaceFor_Throws_When_Assignments_Are_Incomplete()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");

        var act = () => session.InterfaceFor("B1");

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Return_Passes_To_A_Are_Implied_By_Topology_For_B_Roles()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");

        session.PassingPartnerFor("B1", Throw.AnyPass).Should().Be("A");
        session.PassingPartnerFor("B2", Throw.AnyPass).Should().Be("A");
    }
}

[TestFixture]
public class LocalProjectionTests
{
    [Test]
    public void Projects_Global_Results_To_Local_Notation_For_Role_And_Deduplicates()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");

        var globals = new[]
        {
            Siteswap.CreateFromCorrect(8, 6, 8, 6, 7),
            Siteswap.CreateFromCorrect(8, 6, 8, 6, 7),
            Siteswap.CreateFromCorrect(7, 8, 6, 8, 6),
        };

        var locals = session.ProjectLocalResults("B1", globals);

        locals.Should().HaveCount(2);
        locals[0]
            .LocalNotation.Should()
            .Be(Siteswap.CreateFromCorrect(8, 6, 8, 6, 7).GetLocalSiteswap(1, 2).GlobalNotation);
        locals[1]
            .LocalNotation.Should()
            .Be(Siteswap.CreateFromCorrect(7, 8, 6, 8, 6).GetLocalSiteswap(1, 2).GlobalNotation);
    }
}

[TestFixture]
public class SharedRotationTests
{
    [Test]
    public void Rotate_Shifts_Feeder_Throws_And_Pass_Assignments_Together()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");

        session.Rotate(1);

        session.FeederSiteswap.Items.Should().Equal(5, 6, 7);
        session.PassAssignments.Should().Equal("B2", null, "B1");
        session.ArePassAssignmentsComplete.Should().BeTrue();
    }

    [Test]
    public void Rotate_Also_Rotates_Selected_Role_Siteswaps()
    {
        // Period + landing-interface aligned (B1=S,P,S → 676; B2=P,S,S → 766).
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");
        session.SelectSiteswap("B1", Siteswap.CreateFromCorrect(6, 7, 6));
        session.SelectSiteswap("B2", Siteswap.CreateFromCorrect(7, 6, 6));

        session.Rotate(1);

        session.SelectedSiteswap("B1")!.Items.Should().Equal(7, 6, 6);
        session.SelectedSiteswap("B2")!.Items.Should().Equal(6, 6, 7);
    }
}

[TestFixture]
public class StartingClubsTests
{
    [Test]
    public void Starting_Clubs_Split_Left_And_Right_And_Pair_Sums_To_Objects()
    {
        // Period + landing-interface aligned for B1 on 756 (S,P,S → 676).
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");
        var b1Siteswap = Siteswap.CreateFromCorrect(6, 7, 6);
        session.SelectSiteswap("B1", b1Siteswap);

        var b1 = session.StartingClubs("B1");
        var expected = StartingClubDistribution.ForJuggler(b1Siteswap.Items, juggler: 1);

        b1.Should().Be(expected);
        (b1.Left + b1.Right).Should().BeGreaterThan(0);
    }
}
