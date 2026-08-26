using FluentAssertions;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Components.SiteswapLab;

namespace Siteswaps.Generator.Test.Components.SiteswapLab;

[TestFixture]
public class PassingEditorRotationClubCountTests
{
    private static int TotalStartingClubs(PassingEditorState state) =>
        state.People.Select((_, i) => state.StartingClubsFor(i)).Sum(c => c.Left + c.Right);

    private static void AssertTotalClubsInvariantThroughFullRotation(
        PassingEditorState state,
        bool requireValid = true
    )
    {
        if (requireValid)
        {
            state.IsValid.Should().BeTrue("pattern must be valid before rotation invariant check");
        }

        var expected = TotalStartingClubs(state);

        for (var step = 0; step < state.Period; step++)
        {
            TotalStartingClubs(state).Should().Be(expected, $"at rotation step {step}");
            state.Rotate(1);
        }

        if (requireValid)
        {
            state.IsValid.Should().BeTrue("rotation must preserve a valid pattern");
        }
    }

    private static void AssertPersonAHasTwoTwoStartingClubsThroughFullRotation(
        PassingEditorState state
    )
    {
        state.IsValid.Should().BeTrue("pattern must be valid before A start-club invariant check");

        for (var step = 0; step < state.Period; step++)
        {
            state
                .StartingClubsFor(0)
                .Should()
                .Be(
                    new ClubHands(2, 2),
                    $"person A must keep 2/2 start clubs at rotation step {step}"
                );
            state.Rotate(1);
        }

        state.IsValid.Should().BeTrue("rotation must preserve a valid pattern");
    }

    /// <summary>
    /// Three-person feed with person A juggling four clubs (local height 4, all self throws).
    /// Matches Siteswap Lab screenshot: A shows 2/2 start clubs and five purple "4" cells.
    /// </summary>
    private static PassingEditorState CreateThreePersonAFourClubSelfPattern()
    {
        var state = new PassingEditorState();
        state.InitializeThrowsForFirstEntry();

        for (var beat = 0; beat < state.Period; beat++)
        {
            state.SetHeight(0, beat, state.ToGlobalHeight(4));
            state.SetTarget(0, beat, 0);
        }

        state.IsValid.Should().BeTrue("four-club A self pattern must be valid");
        state.StartingClubsFor(0).Should().Be(new ClubHands(2, 2), "A starts with four clubs");

        return state;
    }

    [Test]
    public void Rotate_Keeps_Person_A_Two_Two_Starting_Clubs_For_Three_Person_A_Four_Club_Self_Pattern()
    {
        AssertPersonAHasTwoTwoStartingClubsThroughFullRotation(
            CreateThreePersonAFourClubSelfPattern()
        );
    }

    [Test]
    public void Rotate_Keeps_Total_Starting_Clubs_For_Default_Three_Person_After_Init()
    {
        var state = new PassingEditorState();
        state.InitializeThrowsForFirstEntry();
        AssertTotalClubsInvariantThroughFullRotation(state);
    }

    [Test]
    public void Rotate_Keeps_Total_Starting_Clubs_For_531_Single_Person()
    {
        AssertTotalClubsInvariantThroughFullRotation(new PassingEditorState("531"));
    }

    [Test]
    public void Rotate_Keeps_Total_Starting_Clubs_For_78627_Single_Person()
    {
        AssertTotalClubsInvariantThroughFullRotation(new PassingEditorState("78627"));
    }

    [Test]
    public void Rotate_Keeps_Total_Starting_Clubs_For_531_Three_Person()
    {
        var state = new PassingEditorState("531");
        state.SetPersonCount(3);
        state.InitializeThrowsForFirstEntry();
        AssertTotalClubsInvariantThroughFullRotation(state);
    }

    [Test]
    public void Rotate_Keeps_Total_Starting_Clubs_For_756_Two_Person()
    {
        var state = new PassingEditorState("756");
        state.SetPersonCount(2);
        state.InitializeThrowsForFirstEntry();
        AssertTotalClubsInvariantThroughFullRotation(state);
    }

    [Test]
    public void Rotate_Negative_Steps_Keep_Total_Starting_Clubs()
    {
        var state = new PassingEditorState();
        state.InitializeThrowsForFirstEntry();
        var expected = TotalStartingClubs(state);

        state.Rotate(-1);
        TotalStartingClubs(state).Should().Be(expected);
        state.IsValid.Should().BeTrue();
    }
}
