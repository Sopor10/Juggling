using FluentAssertions;
using Siteswaps.Generator.Components.GenerationWorkflow;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.GenerationWorkflow;

[TestFixture]
public class GenerationWorkflowConfigTests
{
    [Test]
    public void Apply_Locks_Period_And_Jugglers_Onto_WizardState()
    {
        var config = new GenerationWorkflowConfig { Period = 5, NumberOfJugglers = 2 };
        var session = GenerationWorkflowSession.Create(config);

        session.State.Period.Value.Should().Be(5);
        session.State.NumberOfJugglers.Should().Be(2);
        session.IsPeriodEditable.Should().BeFalse();
        session.IsJugglersEditable.Should().BeFalse();
        session.IsPeriodVisible.Should().BeFalse();
        session.IsJugglersVisible.Should().BeFalse();
    }

    [Test]
    public void Unlocked_Inputs_Remain_Editable_And_Visible()
    {
        var session = GenerationWorkflowSession.Create(new GenerationWorkflowConfig());

        session.IsPeriodEditable.Should().BeTrue();
        session.IsJugglersEditable.Should().BeTrue();
        session.IsPeriodVisible.Should().BeTrue();
        session.IsJugglersVisible.Should().BeTrue();

        session.SetPeriod(7);
        session.SetNumberOfJugglers(3);

        session.State.Period.Value.Should().Be(7);
        session.State.NumberOfJugglers.Should().Be(3);
    }

    [Test]
    public void SetPeriod_Is_Rejected_When_Period_Is_Locked()
    {
        var session = GenerationWorkflowSession.Create(
            new GenerationWorkflowConfig { Period = 5, NumberOfJugglers = 2 }
        );

        var act = () => session.SetPeriod(9);

        act.Should().Throw<InvalidOperationException>();
        session.State.Period.Value.Should().Be(5);
    }

    [Test]
    public void SetNumberOfJugglers_Is_Rejected_When_Jugglers_Are_Locked()
    {
        var session = GenerationWorkflowSession.Create(
            new GenerationWorkflowConfig { Period = 5, NumberOfJugglers = 2 }
        );

        var act = () => session.SetNumberOfJugglers(4);

        act.Should().Throw<InvalidOperationException>();
        session.State.NumberOfJugglers.Should().Be(2);
    }

    [Test]
    public void Locked_PassSelf_Interface_Is_Injected_And_Cannot_Be_Removed()
    {
        var interfacePattern = new[] { Throw.AnySelf, Throw.AnyPass, Throw.AnySelf };
        var session = GenerationWorkflowSession.Create(
            new GenerationWorkflowConfig
            {
                Period = 3,
                NumberOfJugglers = 2,
                PassSelfInterface = interfacePattern,
            }
        );

        session.HasLockedInterface.Should().BeTrue();
        session.LockedInterfaceFilterId.Should().NotBeNull();

        var leaf = WizardFilterTree.FindLeaf(
            session.State.FilterTree,
            session.LockedInterfaceFilterId!.Value
        );
        leaf.Should().NotBeNull();
        var pattern = WizardFilterTree
            .Unwrap(leaf!.Filter)
            .Should()
            .BeOfType<NewPatternFilterInformation>()
            .Subject;
        pattern.Pattern.Should().Equal(interfacePattern);

        var act = () => session.RemoveFilter(session.LockedInterfaceFilterId.Value);
        act.Should().Throw<InvalidOperationException>();

        WizardFilterTree
            .FindLeaf(session.State.FilterTree, session.LockedInterfaceFilterId.Value)
            .Should()
            .NotBeNull();
    }

    [Test]
    public void Clubs_And_Throws_Remain_Editable_With_Locks()
    {
        var session = GenerationWorkflowSession.Create(
            new GenerationWorkflowConfig { Period = 5, NumberOfJugglers = 2 }
        );

        session.SetClubs(new Between { MinNumber = 4, MaxNumber = 6 });
        session.State.AllowedThrows.Clear();
        session.State.AllowedThrows.Add(Throw.Self);
        session.State.AllowedThrows.Add(Throw.SinglePass);

        session.State.Clubs.Should().Be(new Between { MinNumber = 4, MaxNumber = 6 });
        session.State.AllowedThrows.Should().Equal(Throw.Self, Throw.SinglePass);
    }

    [Test]
    public void Session_Has_No_Feeding_Surface()
    {
        var type = typeof(GenerationWorkflowSession);
        type.GetProperty("Topology").Should().BeNull();
        type.GetProperty("PassAssignments").Should().BeNull();
        type.GetMethod("AssignPass").Should().BeNull();
        type.GetMethod("InterfaceFor").Should().BeNull();
    }
}

[TestFixture]
public class GenerationWorkflowGenerateTests
{
    [Test]
    public async Task GenerateAsync_Returns_Result_List_Without_Selecting_A_Siteswap()
    {
        var session = GenerationWorkflowSession.Create(
            new GenerationWorkflowConfig { Period = 5, NumberOfJugglers = 2 }
        );
        session.SetClubs(new Between { MinNumber = 6, MaxNumber = 6 });

        var results = await session.GenerateAsync();

        results.Should().NotBeEmpty();
        results.Should().OnlyContain(s => s.Period.Value == 5);
        typeof(GenerationWorkflowSession).GetProperty("SelectedSiteswap").Should().BeNull();
        typeof(GenerationWorkflowSession).GetMethod("SelectSiteswap").Should().BeNull();
    }

    [Test]
    public async Task GenerateAsync_With_Locked_Interface_Only_Yields_Matching_Patterns()
    {
        var required = new[] { PassKind.Pass, PassKind.Self, PassKind.Self };
        var session = GenerationWorkflowSession.Create(
            new GenerationWorkflowConfig
            {
                Period = 3,
                NumberOfJugglers = 2,
                PassSelfInterface = [Throw.AnyPass, Throw.AnySelf, Throw.AnySelf],
            }
        );
        session.SetClubs(new Between { MinNumber = 5, MaxNumber = 5 });
        session.State.AllowedThrows.Clear();
        session.State.AllowedThrows.AddRange([
            Throw.Zip,
            Throw.Hold,
            Throw.Zap,
            Throw.Self,
            Throw.SinglePass,
            Throw.Heff,
            Throw.DoublePass,
        ]);

        var results = await session.GenerateAsync();

        results.Should().NotBeEmpty();
        foreach (var siteswap in results.Take(40))
        {
            MatchesCyclicPassSelf(siteswap.Items, required).Should().BeTrue();
        }
    }

    [Test]
    public async Task GenerateAsync_Uses_Same_FilterTranslation_As_Wizard()
    {
        var session = GenerationWorkflowSession.Create(new GenerationWorkflowConfig());
        session.SetPeriod(5);
        session.SetNumberOfJugglers(2);
        session.SetClubs(new Between { MinNumber = 6, MaxNumber = 6 });

        var viaSession = (await session.GenerateAsync()).Take(40).ToList();
        var viaTranslation = CollectViaFilterTranslation(session.State.Inner, limit: 40);

        viaSession.Should().Equal(viaTranslation);
    }

    [Test]
    public void Wizard_Generation_Path_Uses_Shared_SiteswapListGeneration()
    {
        var wizardSource = File.ReadAllText(
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..",
                "..",
                "..",
                "..",
                "Siteswaps.Generator",
                "Components",
                "WizardPage",
                "WizardPage.razor.cs"
            )
        );

        wizardSource
            .Should()
            .Contain(
                "SiteswapListGeneration.GenerateStreamAsync",
                "WizardPage must call the shared generation workflow"
            );
    }

    private static List<Siteswap> CollectViaFilterTranslation(WizardState state, int limit)
    {
        var found = new List<Siteswap>();
        foreach (var generator in FilterTranslation.CreateGenerators(state))
        {
            foreach (var siteswap in generator.Generate())
            {
                found.Add(siteswap);
                if (found.Count >= limit)
                {
                    return found;
                }
            }
        }

        return found;
    }

    private static bool MatchesCyclicPassSelf(IReadOnlyList<int> heights, PassKind[] required)
    {
        for (var offset = 0; offset < heights.Count; offset++)
        {
            var ok = true;
            for (var i = 0; i < required.Length; i++)
            {
                var kind =
                    heights[(i + offset) % heights.Count] % 2 == 0 ? PassKind.Self : PassKind.Pass;
                if (kind != required[i])
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
            {
                return true;
            }
        }

        return false;
    }

    private enum PassKind
    {
        Pass,
        Self,
    }
}
