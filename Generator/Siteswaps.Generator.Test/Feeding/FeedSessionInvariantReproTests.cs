using System.Reflection;
using FluentAssertions;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.Feeding;

/// <summary>
/// Round-2 adjudicated specs for NormalFeedSession (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class FeedSessionInvariantReproTests
{
    [Test]
    public void FromFeederSiteswap_Rejects_Feeder_Without_At_Least_One_Pass()
    {
        var act = () => NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(6, 6, 6));

        act.Should().Throw<ArgumentException>("feeder must be a two-person pattern with ≥2 passes");
    }

    [Test]
    public void FromFeederSiteswap_Rejects_Feeder_With_Only_One_Pass()
    {
        // a=10 (self), b=11 (pass), c=12 (self) — letter notation that is landing-valid but not a feed.
        var act = () =>
            NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(10, 11, 12));

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void FromFeederSiteswap_Rejects_All_Self_Feeder_864()
    {
        var act = () => NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(8, 6, 4));

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CanGenerate_Is_False_When_All_Passes_Are_Assigned_To_A_Single_Fedee()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B1");

        session.ArePassAssignmentsComplete.Should().BeTrue();
        session.CanGenerate.Should().BeFalse("all passes to one fedee leaves the other all-self");
    }

    [Test]
    public void RemainingPassBeats_Empty_Does_Not_Imply_CanGenerate_When_One_Fedee_Starved()
    {
        // UI contract: empty RemainingPassBeats ≠ ready; still need both fedees + block reason.
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B1");

        session.RemainingPassBeats.Should().BeEmpty();
        session.CanGenerate.Should().BeFalse();
        session.GenerationBlockReason.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void StartingClubs_For_Unknown_Role_Fails_Fast_With_Argument()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));

        var act = () => session.StartingClubs("foo");

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("role");
    }

    [Test]
    public void PassingPartnerFor_A_Is_Beat_Aware_Beat0_B1_Beat1_B2()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");

        var beatAware = typeof(NormalFeedSession)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.Name == "PassingPartnerFor")
            .FirstOrDefault(m =>
                m.GetParameters()
                    .Any(p => p.Name is "beatIndex" or "beat" || p.ParameterType == typeof(int))
            );

        beatAware
            .Should()
            .NotBeNull(
                "PassingPartnerFor(A) must take a beat index (or sequence); partners differ per pass beat"
            );

        var parameters = beatAware!.GetParameters();
        object?[] args0;
        object?[] args1;
        if (parameters.Length == 3 && parameters[1].ParameterType == typeof(Throw))
        {
            args0 = ["A", Throw.AnyPass, 0];
            args1 = ["A", Throw.AnyPass, 1];
        }
        else if (parameters.Length == 2 && parameters[1].ParameterType == typeof(int))
        {
            args0 = ["A", 0];
            args1 = ["A", 1];
        }
        else
        {
            Assert.Fail($"Unexpected PassingPartnerFor overload: {beatAware}");
            return;
        }

        beatAware.Invoke(session, args0).Should().Be("B1");
        beatAware.Invoke(session, args1).Should().Be("B2");
    }

    [Test]
    public void SelectSiteswap_Rejects_Unknown_Role()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));

        var act = () => session.SelectSiteswap("X", Siteswap.CreateFromCorrect(7, 5, 6));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void SelectSiteswap_Rejects_Period_Mismatch_With_Feeder()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");

        var act = () => session.SelectSiteswap("B1", Siteswap.CreateFromCorrect(8, 6, 8, 6, 7));

        // 756 period 3 vs 86867 period 5
        act.Should()
            .Throw<ArgumentException>("selected siteswap period must match the feeder/interface");
    }

    [Test]
    public void SelectSiteswap_Rejects_When_Pass_Assignments_Incomplete()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");

        var act = () => session.SelectSiteswap("B1", Siteswap.CreateFromCorrect(7, 5, 6));

        act.Should()
            .Throw<InvalidOperationException>(
                "selection before complete pass assignments must be rejected"
            );
    }

    [Test]
    public void SelectSiteswap_Rejects_Pass_On_Wrong_Landing_Beat()
    {
        var session = NormalFeedSession.FromFeederSiteswap(
            Siteswap.CreateFromCorrect(7, 8, 6, 2, 7)
        );
        session.AssignPass(0, "B1");
        session.AssignPass(4, "B2");

        // Landing interface for B1 is S,S,P,S,S — a pass only at beat 0 must not satisfy it.
        var act = () => session.SelectSiteswap("B1", Siteswap.CreateFromCorrect(7, 6, 6, 6, 6));

        act.Should()
            .Throw<ArgumentException>(
                "selection must place passes on the landing beats required by the interface"
            );
    }

    [Test]
    public void SelectSiteswap_Invalidates_Selection_When_Pass_Assignments_Change()
    {
        var session = NormalFeedSession.FromFeederSiteswap(
            Siteswap.CreateFromCorrect(7, 8, 6, 2, 7)
        );
        session.AssignPass(0, "B1");
        session.AssignPass(4, "B2");
        // Landing interface B1 = S,S,P,S,S
        session.SelectSiteswap("B1", Siteswap.CreateFromCorrect(6, 6, 7, 6, 6));

        session.AssignPass(0, "B2");
        session.AssignPass(4, "B1");

        session
            .SelectedSiteswap("B1")
            .Should()
            .BeNull("changing assignments must invalidate stale selections");
    }

    [Test]
    public void Rotate_CoRotates_Selections_And_Keeps_Them_Aligned()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");
        // B1 landing interface S,P,S
        session.SelectSiteswap("B1", Siteswap.CreateFromCorrect(6, 7, 6));

        session.Rotate(1);

        session.FeederSiteswap.Items.Should().Equal(5, 6, 7);
        session.SelectedSiteswap("B1")!.Items.Should().Equal(7, 6, 6);
        session.SelectedSiteswap("B1").Should().NotBeNull();
    }

    [Test]
    public void PassAssignments_Is_Not_Mutably_Leaked_As_Underlying_Array()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));

        var asArray = session.PassAssignments as string?[];
        asArray.Should().BeNull("PassAssignments must not expose the mutable backing store");
    }

    [Test]
    public void Clubs_Default_Matches_Wizard_Window_And_Allows_Generation()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");

        session.ClubsB1.Should().Be(new Between { MinNumber = 5, MaxNumber = 7 });
        session.ClubsB2.Should().Be(new Between { MinNumber = 5, MaxNumber = 7 });

        session
            .CanGenerate.Should()
            .BeTrue("default club bounds must align with slider UI and allow generate");
    }

    [Test]
    public void Reset_Restores_Club_Bounds_To_Session_Defaults()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.ClubsB1 = new Between { MinNumber = 3, MaxNumber = 5 };
        session.ClubsB2 = new Between { MinNumber = 2, MaxNumber = 4 };
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");

        session.Reset();

        session.ClubsB1.Should().Be(new Between { MinNumber = 5, MaxNumber = 7 });
        session.ClubsB2.Should().Be(new Between { MinNumber = 5, MaxNumber = 7 });
    }

    [Test]
    public void GenerationBlockReason_Exposes_Stable_Code_Not_Only_Free_Text()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B1");

        var type = typeof(NormalFeedSession);
        var coded =
            type.GetProperty("GenerationBlockCode")
            ?? type.GetProperty("GenerationBlockReasonCode");
        var reasonProperty = type.GetProperty("GenerationBlockReason");
        var reasonIsEnum = reasonProperty is not null && reasonProperty.PropertyType.IsEnum;

        (coded is not null || reasonIsEnum)
            .Should()
            .BeTrue(
                "UI/i18n needs a stable GenerationBlockCode (or enum reason), not English free-text alone"
            );
    }

    [Test]
    public void ProjectLocalResults_Dedupes_Exact_LocalNotation_Strings_Only()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");

        var globals = new[]
        {
            Siteswap.CreateFromCorrect(8, 6, 7),
            Siteswap.CreateFromCorrect(8, 6, 7),
            Siteswap.CreateFromCorrect(6, 7, 8),
        };

        var locals = session.ProjectLocalResults("B1", globals);

        // Exact-string dedup is Soll; rotational variants with different notation stay distinct.
        locals.Should().HaveCount(2);
        locals.Select(l => l.LocalNotation).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void StartingClubs_Missing_Selection_Is_Fail_Soft_For_Ui()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");
        session.SelectSiteswap("B1", Siteswap.CreateFromCorrect(6, 7, 6));
        session.ClearPass(0);

        var tryMethod = typeof(NormalFeedSession).GetMethod(
            "TryStartingClubs",
            BindingFlags.Instance | BindingFlags.Public
        );
        var startingClubs = typeof(NormalFeedSession).GetMethod(
            "StartingClubs",
            BindingFlags.Instance | BindingFlags.Public
        );
        var returnsNullable =
            startingClubs is not null
            && Nullable.GetUnderlyingType(startingClubs.ReturnType) == typeof(ClubHands);

        if (tryMethod is not null)
        {
            var args = new object?[] { "B1", null };
            var ok = (bool)tryMethod.Invoke(session, args)!;
            ok.Should().BeFalse();
            return;
        }

        if (returnsNullable)
        {
            startingClubs!.Invoke(session, ["B1"]).Should().BeNull();
            return;
        }

        // Until TryStartingClubs / nullable exists: must not crash the UI path.
        var act = () => session.StartingClubs("B1");
        act.Should()
            .NotThrow(
                "after ClearPass / missing selection, StartingClubs must be fail-soft for UI"
            );
    }
}

[TestFixture]
public class FeedSessionApiGapReproTests
{
    [Test]
    public void Session_Exposes_ClearPass_To_Unassign_A_Pass_Beat()
    {
        typeof(NormalFeedSession)
            .GetMethod("ClearPass", BindingFlags.Instance | BindingFlags.Public)
            .Should()
            .NotBeNull("reviewers require ClearPass / Unassign");
    }

    [Test]
    public void Session_Exposes_Reset_Back_To_Original_Feeder()
    {
        typeof(NormalFeedSession)
            .GetMethod("Reset", BindingFlags.Instance | BindingFlags.Public)
            .Should()
            .NotBeNull("Rotate mutates the feeder; Reset to the original feeder is required");
    }

    [Test]
    public void Session_Exposes_RemainingPassBeats_Or_GenerationBlockReason()
    {
        var type = typeof(NormalFeedSession);
        var hasProgress =
            type.GetProperty("RemainingPassBeats") is not null
            || type.GetProperty("GenerationBlockReason") is not null
            || type.GetMethod("GenerationBlockReason") is not null;

        hasProgress
            .Should()
            .BeTrue("UX needs RemainingPassBeats or GenerationBlockReason for incomplete feeds");
    }

    [Test]
    public void ClearPass_Behaviour_Unassigns_Partner_And_Blocks_Generation()
    {
        var clear = typeof(NormalFeedSession).GetMethod(
            "ClearPass",
            BindingFlags.Instance | BindingFlags.Public
        );
        clear.Should().NotBeNull();

        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");

        clear!.Invoke(session, [0]);

        session.PassAssignments[0].Should().BeNull();
        session.CanGenerate.Should().BeFalse();
    }
}

[TestFixture]
public class FeedSessionBoundaryReproTests
{
    [Test]
    public void AssignPass_Rejects_Out_Of_Range_Beat_Index()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));

        var act = () => session.AssignPass(99, "B1");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void AssignPass_Rejects_Invalid_Partner_Role()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));

        var act = () => session.AssignPass(0, "C");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void PassingPartnerFor_Rejects_Self_Throw_Kind()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));

        var act = () => session.PassingPartnerFor("B1", Throw.AnySelf);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void PassingPartnerFor_TwoArg_Overload_Rejects_Role_A()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");

        session.PassingPartnerFor("A", Throw.AnyPass, 0).Should().Be("B1");
        session.PassingPartnerFor("A", Throw.AnyPass, 1).Should().Be("B2");

        var act = () => session.PassingPartnerFor("A", Throw.AnyPass);

        // Soll: 2-arg for A must hard-fail (beat-unaware footgun); callers use 3-arg.
        act.Should()
            .Throw<InvalidOperationException>(
                "2-arg PassingPartnerFor for A must fail; partners differ per pass beat — use 3-arg"
            );
    }

    [Test]
    public void Rotate_Supports_Negative_Steps()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");

        session.Rotate(-1);

        session.FeederSiteswap.Items.Should().Equal(6, 7, 5);
        session.PassAssignments.Should().Equal(null, "B1", "B2");
    }

    [Test]
    public void ProjectLocalResults_Works_For_B2()
    {
        var session = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        session.AssignPass(0, "B1");
        session.AssignPass(1, "B2");

        var globals = new[] { Siteswap.CreateFromCorrect(8, 6, 8, 6, 7) };
        var locals = session.ProjectLocalResults("B2", globals);

        locals.Should().HaveCount(1);
        locals[0]
            .LocalNotation.Should()
            .Be(Siteswap.CreateFromCorrect(8, 6, 8, 6, 7).GetLocalSiteswap(1, 2).GlobalNotation);
    }

    [Test]
    public void SelectSiteswap_Should_Align_Or_Validate_Against_Interface()
    {
        var session = NormalFeedSession.FromFeederSiteswap(
            Siteswap.CreateFromCorrect(7, 8, 6, 2, 7)
        );
        session.AssignPass(0, "B1");
        session.AssignPass(4, "B2");

        // All-self pattern cannot satisfy B1's landing interface which contains a pass.
        var act = () => session.SelectSiteswap("B1", Siteswap.CreateFromCorrect(6, 6, 6, 6, 6));

        act.Should()
            .Throw<ArgumentException>("selection must be consistent with the role interface");
    }
}
