using FluentAssertions;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.Feeding;

/// <summary>Throw-time vs landing-time interface semantics and collision behaviour.</summary>
[TestFixture]
public class FeedInterfaceContractTests
{
    [Test]
    public void InterfaceFor_Returns_Landing_Pattern_Not_Throw_Time_For_78627_B1_At_Beat_0()
    {
        // Spec: InterfaceFor is landing-time (S,S,P,S,S); throw-time for B1@0 would be P,S,S,S,S.
        var session = NormalFeedSession.FromFeederSiteswap(
            Siteswap.CreateFromCorrect(7, 8, 6, 2, 7)
        );
        session.AssignPass(0, "B1");
        session.AssignPass(4, "B2");

        session
            .InterfaceFor("B1")
            .Should()
            .Equal(Throw.AnySelf, Throw.AnySelf, Throw.AnyPass, Throw.AnySelf, Throw.AnySelf);
    }

    [Test]
    public void Throw_Time_Pattern_For_78627_B1_At_Beat_0_Is_Pass_Then_Selfs()
    {
        // Documents the throw-time view reviewers expect to be available / transparent.
        var heights = new[] { 7, 8, 6, 2, 7 };
        var throwTime = new[]
        {
            Throw.AnyPass,
            Throw.AnySelf,
            Throw.AnySelf,
            Throw.AnySelf,
            Throw.AnySelf,
        };

        var landing = FeedInterface.RotateToLanding(heights, throwTime);
        landing
            .Should()
            .Equal(Throw.AnySelf, Throw.AnySelf, Throw.AnyPass, Throw.AnySelf, Throw.AnySelf);
        landing.Should().NotEqual(throwTime);
    }

    [Test]
    public void Session_Exposes_Throw_Time_Interface_In_Addition_To_Landing_Interface()
    {
        // Desired API (findings #2/#26/#36/#47): consumers must not guess landing vs throw-time.
        var throwTimeApi = typeof(NormalFeedSession)
            .GetMethods()
            .Any(m =>
                m.Name is "ThrowTimeInterfaceFor" or "InterfaceAtThrowTime" or "ThrowTimeFor"
            );

        throwTimeApi
            .Should()
            .BeTrue(
                "NormalFeedSession should expose an explicit throw-time interface API alongside landing InterfaceFor"
            );
    }

    [Test]
    public void RotateToLanding_Detects_Landing_Collisions_Instead_Of_Last_Write_Wins()
    {
        // (0+5)%3=2 and (1+7)%3=2 both receive P → last-write-wins today (findings #5/#16).
        int[] heights = [5, 7, 3];
        Throw[] throwTime = [Throw.AnyPass, Throw.AnyPass, Throw.AnySelf];

        var act = () => FeedInterface.RotateToLanding(heights, throwTime);

        act.Should()
            .Throw<InvalidOperationException>(
                "landing collisions must be rejected, not silently overwritten"
            );
    }
}
