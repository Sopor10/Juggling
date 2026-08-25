using FluentAssertions;
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

    [Test]
    public void Rotate_Keeps_Total_Starting_Clubs_For_Default_Three_Person_Before_Init()
    {
        var state = new PassingEditorState();
        AssertTotalClubsInvariantThroughFullRotation(state, requireValid: false);
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
