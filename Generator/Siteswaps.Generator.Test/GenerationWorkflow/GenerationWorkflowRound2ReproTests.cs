using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Components.GenerationWorkflow;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.GenerationWorkflow;

/// <summary>
/// Round-2 review-finding repros for reusable-generation (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class GenerationWorkflowRound2ReproTests
{
    [Test]
    public void Locked_Interface_Is_Hidden_From_Host_Ui_Surface()
    {
        // Finding #1 (High): locked P/S interface appears in FilterList with Edit/Remove — Plan: hide.
        var session = GenerationWorkflowSession.Create(
            new GenerationWorkflowConfig
            {
                Period = 3,
                NumberOfJugglers = 2,
                PassSelfInterface = [Throw.AnySelf, Throw.AnyPass, Throw.AnySelf],
            }
        );

        session.HasLockedInterface.Should().BeTrue();

        var isInterfaceVisible = typeof(GenerationWorkflowSession).GetProperty(
            "IsInterfaceVisible",
            BindingFlags.Instance | BindingFlags.Public
        );
        var hasDisplayTreeApi =
            typeof(GenerationWorkflowSession).GetProperty(
                "EditableFilterTree",
                BindingFlags.Instance | BindingFlags.Public
            )
                is not null
            || typeof(GenerationWorkflowSession).GetProperty(
                "VisibleFilterTree",
                BindingFlags.Instance | BindingFlags.Public
            )
                is not null
            || typeof(GenerationWorkflowSession).GetMethod(
                "FilterTreeForDisplay",
                BindingFlags.Instance | BindingFlags.Public
            )
                is not null;

        var hostRazor = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine("Components", "GenerationWorkflow", "ConfiguredGenerationWorkflow.razor")
        );

        var apiHidesLocked =
            (isInterfaceVisible is not null && (bool)isInterfaceVisible.GetValue(session)! == false)
            || hasDisplayTreeApi;

        var markupHidesLocked =
            hostRazor.Contains("IsInterfaceVisible", StringComparison.Ordinal)
            || hostRazor.Contains("LockedInterfaceFilterId", StringComparison.Ordinal)
            || hostRazor.Contains("HiddenFilter", StringComparison.Ordinal)
            || hostRazor.Contains("EditableFilterTree", StringComparison.Ordinal)
            || hostRazor.Contains("VisibleFilterTree", StringComparison.Ordinal)
            || hostRazor.Contains("FilterTreeForDisplay", StringComparison.Ordinal);

        (apiHidesLocked || markupHidesLocked)
            .Should()
            .BeTrue(
                "Plan: locked Pass/Self interface must be hidden from Host UI (IsInterfaceVisible=false / display tree without locked leaf / markup gate)"
            );
    }

    [Test]
    public void Host_FilterList_Wires_Edit_Remove_And_Add_For_Extra_Filters()
    {
        // Finding #2 (High): Host renders FilterList without Edit/Remove/Add — Plan: extra filters editable.
        var hostRazor = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine("Components", "GenerationWorkflow", "ConfiguredGenerationWorkflow.razor")
        );
        var hostCode = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine(
                "Components",
                "GenerationWorkflow",
                "ConfiguredGenerationWorkflow.razor.cs"
            )
        );

        var wiresEdit =
            hostRazor.Contains("Edit=", StringComparison.Ordinal)
            || hostRazor.Contains("Edit=\"", StringComparison.Ordinal);
        var wiresRemove =
            hostRazor.Contains("RemoveRequested", StringComparison.Ordinal)
            || hostCode.Contains("RemoveFilter", StringComparison.Ordinal);
        var wiresAdd =
            hostRazor.Contains("AddInGroup", StringComparison.Ordinal)
            || hostRazor.Contains("wizard-add-filter", StringComparison.Ordinal)
            || hostRazor.Contains("OpenAddFilter", StringComparison.Ordinal)
            || hostCode.Contains("AddFilter", StringComparison.Ordinal)
            || hostCode.Contains("FilterBottomSheet", StringComparison.Ordinal);

        wiresEdit.Should().BeTrue("FilterList Edit must be wired so extra filters can be edited");
        wiresRemove
            .Should()
            .BeTrue("FilterList RemoveRequested must be wired (Session.RemoveFilter)");
        wiresAdd
            .Should()
            .BeTrue("Host must expose Add filter (AddInGroup / add button / bottom sheet)");
    }

    [Test]
    public void ToGenerationWorkflowConfig_Locks_Landing_Interface_Compatible_With_SelectSiteswap()
    {
        // Finding #3 (High): Factory locks ThrowTime + Global; SelectSiteswap validates Landing.
        var feed = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 8, 6, 2, 7));
        feed.AssignPass(0, "B1");
        feed.AssignPass(4, "B2");

        var throwTime = feed.ThrowTimeInterfaceFor("B1");
        var landing = feed.InterfaceFor("B1");
        throwTime
            .Should()
            .NotEqual(landing, "precondition: throw-time and landing must diverge for this feeder");

        var config = feed.ToGenerationWorkflowConfig("B1");
        config
            .PassSelfInterface.Should()
            .Equal(
                landing,
                "ToGenerationWorkflowConfig must lock the landing interface that SelectSiteswap validates"
            );
    }

    [Test]
    public async Task Factory_Generated_Results_Must_All_Pass_SelectSiteswap_Landing_Check()
    {
        // Finding #3 (High) behavioural: Global+ThrowTime yields rotations that fail absolute landing selection.
        var feed = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 8, 6, 2, 7));
        feed.AssignPass(0, "B1");
        feed.AssignPass(4, "B2");

        var config = feed.ToGenerationWorkflowConfig("B1");
        var session = GenerationWorkflowSession.Create(config);
        session.SetClubs(new Between { MinNumber = 5, MaxNumber = 7 });

        var results = await session.GenerateAsync();
        results.Should().NotBeEmpty();

        foreach (var siteswap in results.Take(40))
        {
            var act = () => feed.SelectSiteswap("B1", siteswap);
            act.Should()
                .NotThrow(
                    $"generated {string.Join(",", siteswap.Items)} must satisfy landing SelectSiteswap"
                );
        }
    }

    [Test]
    public void Host_Exposes_Cancel_Busy_Ui_Or_CancelGeneration_Api()
    {
        // Finding #4 (Medium): Generate busy/cancel in Host (or CancelGeneration API).
        var hostRazor = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine("Components", "GenerationWorkflow", "ConfiguredGenerationWorkflow.razor")
        );
        var hostType = typeof(ConfiguredGenerationWorkflow);

        var hasCancelButton =
            hostRazor.Contains("Cancel", StringComparison.OrdinalIgnoreCase)
            && (
                hostRazor.Contains("onclick", StringComparison.OrdinalIgnoreCase)
                || hostRazor.Contains("CancelGeneration", StringComparison.Ordinal)
                || hostRazor.Contains("OnCancel", StringComparison.Ordinal)
            );
        var hasCancelApi =
            hostType.GetMethod("CancelGeneration", BindingFlags.Instance | BindingFlags.Public)
                is not null
            || hostType.GetMethod("Cancel", BindingFlags.Instance | BindingFlags.Public)
                is not null;

        (hasCancelButton || hasCancelApi)
            .Should()
            .BeTrue(
                "Host must offer Cancel UI while generating and/or a public CancelGeneration API"
            );
    }

    [Test]
    public void Host_Disables_Controls_While_IsGenerating()
    {
        // Finding #5 (Medium): only Generate button is disabled; Period/Clubs/Throws/Filters stay interactive.
        var hostRazor = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine("Components", "GenerationWorkflow", "ConfiguredGenerationWorkflow.razor")
        );

        var isGeneratingDisableCount =
            hostRazor.Split("disabled=\"@IsGenerating\"", StringSplitOptions.None).Length - 1;
        var gatesEditingControls =
            isGeneratingDisableCount >= 2
            || hostRazor.Contains("@if (!IsGenerating)", StringComparison.Ordinal)
            || hostRazor.Contains("@if(!IsGenerating)", StringComparison.Ordinal)
            || hostRazor.Contains("readonly=\"@IsGenerating\"", StringComparison.OrdinalIgnoreCase);

        gatesEditingControls
            .Should()
            .BeTrue(
                "while IsGenerating, Period/Jugglers/Clubs/Throws/Filters must be disabled or non-interactive — not only the Generate button"
            );
    }

    [Test]
    public void ToGenerationWorkflowConfig_Handoff_Carries_Role_Clubs()
    {
        // Finding #6 (Medium): Clubs not in ToGenerationWorkflowConfig / clear handoff.
        var feed = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        feed.AssignPass(0, "B1");
        feed.AssignPass(1, "B2");
        feed.ClubsB1 = new Between { MinNumber = 3, MaxNumber = 5 };
        feed.ClubsB2 = new Between { MinNumber = 2, MaxNumber = 4 };

        var config = feed.ToGenerationWorkflowConfig("B1");
        var clubsOnConfig = typeof(GenerationWorkflowConfig).GetProperty("Clubs");
        if (clubsOnConfig is not null)
        {
            clubsOnConfig.GetValue(config).Should().Be(feed.ClubsB1);
            return;
        }

        var sessionFactory = typeof(NormalFeedSession)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m =>
                m.ReturnType == typeof(GenerationWorkflowSession)
                && m.GetParameters().Any(p => p.ParameterType == typeof(string))
            );

        if (sessionFactory is not null)
        {
            var session = (GenerationWorkflowSession)sessionFactory.Invoke(feed, ["B1"])!;
            session.State.Clubs.Should().Be(feed.ClubsB1);
            return;
        }

        // Fallback Soll: Create(config) alone must apply role clubs without a separate SetClubs call.
        var sessionFromConfig = GenerationWorkflowSession.Create(config);
        sessionFromConfig
            .State.Clubs.Should()
            .Be(
                feed.ClubsB1,
                "Feeding→GenerationWorkflow handoff must carry ClubsB1/ClubsB2 for the role"
            );
    }

    [Test]
    public async Task Config_Change_During_Generate_Does_Not_Fire_Stale_OnResultsReady()
    {
        // Finding #7 (Medium): Config swap mid-generate still delivers old list via OnResultsReady.
        var host = new ConfiguredGenerationWorkflow();
        SetParameter(
            host,
            nameof(ConfiguredGenerationWorkflow.Config),
            new GenerationWorkflowConfig { Period = 5, NumberOfJugglers = 2 }
        );
        InvokeOnParametersSet(host);
        host.Session.SetClubs(new Between { MinNumber = 5, MaxNumber = 8 });

        IReadOnlyList<Siteswap>? delivered = null;
        var readyCount = 0;
        SetParameter(
            host,
            nameof(ConfiguredGenerationWorkflow.OnResultsReady),
            new EventCallback<IReadOnlyList<Siteswap>>(
                null,
                (Func<IReadOnlyList<Siteswap>, Task>)(
                    results =>
                    {
                        delivered = results;
                        Interlocked.Increment(ref readyCount);
                        return Task.CompletedTask;
                    }
                )
            )
        );

        var generateTask = host.GenerateAsync();
        await Task.Yield();

        SetParameter(
            host,
            nameof(ConfiguredGenerationWorkflow.Config),
            new GenerationWorkflowConfig { Period = 3, NumberOfJugglers = 2 }
        );
        InvokeOnParametersSet(host);

        await generateTask;

        if (readyCount == 0)
        {
            // Acceptable Soll: config change cancels / suppresses delivery.
            return;
        }

        delivered.Should().NotBeNull();
        delivered!
            .Should()
            .OnlyContain(
                s => s.Period.Value == host.Session.Config.Period,
                "OnResultsReady must not deliver stale results for a superseded Config"
            );
        host.Session.Config.Period.Should().Be(3);
    }

    [Test]
    public async Task PassSelfInterface_Caller_List_Is_Snapshotted_On_Create()
    {
        // Finding #8 (Medium): PassSelfInterface mutable list aliasing after Create.
        var mutable = new List<Throw> { Throw.AnyPass, Throw.AnySelf, Throw.AnySelf };
        var session = GenerationWorkflowSession.Create(
            new GenerationWorkflowConfig
            {
                Period = 3,
                NumberOfJugglers = 2,
                PassSelfInterface = mutable,
            }
        );
        session.SetClubs(new Between { MinNumber = 5, MaxNumber = 5 });

        mutable[0] = Throw.AnySelf;
        mutable[1] = Throw.AnyPass;
        mutable[2] = Throw.AnyPass;

        await session.GenerateAsync();

        var leaf = WizardFilterTree.FindLeaf(
            session.State.FilterTree,
            session.LockedInterfaceFilterId!.Value
        );
        var pattern = (NewPatternFilterInformation)WizardFilterTree.Unwrap(leaf!.Filter);
        pattern
            .Pattern.Should()
            .Equal(
                [Throw.AnyPass, Throw.AnySelf, Throw.AnySelf],
                "Create must snapshot PassSelfInterface; caller mutations must not affect locked filter"
            );
    }

    [Test]
    public void State_Inner_Cannot_Bypass_Period_Lock_Without_EnforceLocks()
    {
        // Finding #9 (Low/Medium): State.Inner bypasses soft/hard lock wrappers until Generate.
        var session = GenerationWorkflowSession.Create(
            new GenerationWorkflowConfig { Period = 5, NumberOfJugglers = 2 }
        );

        session.State.Inner.Period = new Period(9);

        session
            .State.Period.Value.Should()
            .Be(5, "Inner must not bypass locked Period without going through EnforceLocks");
    }

    [Test]
    public void Wizard_Embeds_ConfiguredGenerationWorkflow()
    {
        // Finding #10 (Low/Medium): Plan — Wizard adopts hostable ConfiguredGenerationWorkflow.
        var wizardMarkup = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor")
        );

        wizardMarkup
            .Should()
            .Contain(
                "ConfiguredGenerationWorkflow",
                "Plan Soll: WizardPage embeds <ConfiguredGenerationWorkflow> (one configure surface)"
            );
    }

    private static void SetParameter(ConfiguredGenerationWorkflow host, string name, object? value)
    {
        typeof(ConfiguredGenerationWorkflow).GetProperty(name)!.SetValue(host, value);
    }

    private static void InvokeOnParametersSet(ConfiguredGenerationWorkflow host)
    {
        var method = typeof(ConfiguredGenerationWorkflow).GetMethod(
            "OnParametersSet",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        method.Should().NotBeNull();
        method!.Invoke(host, null);
    }
}
