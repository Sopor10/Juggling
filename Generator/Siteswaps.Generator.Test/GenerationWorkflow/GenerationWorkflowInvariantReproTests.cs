using System.Reflection;
using FluentAssertions;
using Siteswaps.Generator.Components.Feeding;
using Siteswaps.Generator.Components.GenerationWorkflow;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.GenerationWorkflow;

/// <summary>
/// Review-finding repros for reusable-generation (desired Soll; no production fixes here).
/// </summary>
[TestFixture]
public class GenerationWorkflowInvariantReproTests
{
    [Test]
    public async Task EnforceLocks_Restores_Locked_Interface_Content_After_ReplaceLeaf()
    {
        // Finding #1: EnforceLocks only re-adds a missing leaf; ReplaceLeaf can mutate content.
        var interfacePattern = new[] { Throw.AnySelf, Throw.AnyPass, Throw.AnySelf };
        var session = GenerationWorkflowSession.Create(
            new GenerationWorkflowConfig
            {
                Period = 3,
                NumberOfJugglers = 2,
                PassSelfInterface = interfacePattern,
            }
        );
        session.SetClubs(new Between { MinNumber = 5, MaxNumber = 5 });

        var lockedId = session.LockedInterfaceFilterId!.Value;
        session.State.FilterTree = WizardFilterTree.ReplaceLeaf(
            session.State.FilterTree,
            lockedId,
            new InterfaceFilterInformation(
                [Throw.AnyPass, Throw.AnyPass, Throw.AnyPass],
                AllowRotation: true
            )
        );

        await session.GenerateAsync();

        var leaf = WizardFilterTree.FindLeaf(session.State.FilterTree, lockedId);
        leaf.Should().NotBeNull();
        var pattern = WizardFilterTree
            .Unwrap(leaf!.Filter)
            .Should()
            .BeOfType<InterfaceFilterInformation>()
            .Subject;
        pattern
            .Landing.Should()
            .Equal(
                interfacePattern,
                "locked Pass/Self interface content must be restored before generate"
            );
    }

    [Test]
    public async Task EnforceLocks_Restores_Locked_Interface_After_InPlace_Pattern_Mutation()
    {
        // Finding #1 (variant): InterfaceFilterInformation.Landing is a mutable list.
        var interfacePattern = new[] { Throw.AnySelf, Throw.AnyPass, Throw.AnySelf };
        var session = GenerationWorkflowSession.Create(
            new GenerationWorkflowConfig
            {
                Period = 3,
                NumberOfJugglers = 2,
                PassSelfInterface = interfacePattern,
            }
        );
        session.SetClubs(new Between { MinNumber = 5, MaxNumber = 5 });

        var lockedId = session.LockedInterfaceFilterId!.Value;
        var leaf = WizardFilterTree.FindLeaf(session.State.FilterTree, lockedId)!;
        var pattern = (InterfaceFilterInformation)WizardFilterTree.Unwrap(leaf.Filter);
        pattern.Landing.Clear();
        pattern.Landing.AddRange([Throw.AnyPass, Throw.AnyPass, Throw.AnyPass]);

        await session.GenerateAsync();

        var after = (InterfaceFilterInformation)
            WizardFilterTree.Unwrap(
                WizardFilterTree.FindLeaf(session.State.FilterTree, lockedId)!.Filter
            );
        after
            .Landing.Should()
            .Equal(interfacePattern, "in-place mutation of locked interface must not stick");
    }

    [Test]
    public void Locked_Period_And_Jugglers_Cannot_Be_Mutated_Via_State_Directly()
    {
        // Finding #2: soft locks — State setters bypass Ensure* until Generate EnforceLocks.
        var session = GenerationWorkflowSession.Create(
            new GenerationWorkflowConfig { Period = 5, NumberOfJugglers = 2 }
        );

        session.State.Period = new Period(9);
        session.State.NumberOfJugglers = 4;

        session.State.Period.Value.Should().Be(5, "locked Period must not be writable via State");
        session
            .State.NumberOfJugglers.Should()
            .Be(2, "locked jugglers must not be writable via State");
    }

    [Test]
    public void PassSelfInterface_Without_Period_Is_Rejected()
    {
        // Finding #5: length check only runs when Period is also set.
        var act = () =>
            GenerationWorkflowSession.Create(
                new GenerationWorkflowConfig
                {
                    PassSelfInterface = [Throw.AnyPass, Throw.AnySelf, Throw.AnySelf],
                }
            );

        act.Should()
            .Throw<ArgumentException>("PassSelfInterface requires Period (and matching length)");
    }

    [Test]
    public void Feeding_Exposes_Factory_Mapping_To_GenerationWorkflowConfig()
    {
        // Finding #6: no Feeding → GenerationWorkflowConfig handoff API.
        var feed = NormalFeedSession.FromFeederSiteswap(Siteswap.CreateFromCorrect(7, 5, 6));
        feed.AssignPass(0, "B1");
        feed.AssignPass(1, "B2");

        var factory = typeof(NormalFeedSession)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
            .Concat(
                typeof(GenerationWorkflowConfig).GetMethods(
                    BindingFlags.Public | BindingFlags.Static
                )
            )
            .FirstOrDefault(IsFeedToGenerationWorkflowConfigFactory);

        factory
            .Should()
            .NotBeNull(
                "expected NormalFeedSession→GenerationWorkflowConfig factory (e.g. ToGenerationWorkflowConfig / FromFeedSession)"
            );

        var config = InvokeFeedConfigFactory(factory!, feed, "B1");
        config.Period.Should().Be(feed.FeederSiteswap.Items.Length);
        config.NumberOfJugglers.Should().Be(2);
        config.PassSelfInterface.Should().NotBeNull();
        config
            .PassSelfInterface!.Should()
            .Equal(
                feed.PartialInterfaceFor("B1"),
                "factory should lock forced-self landing beats for the fedee"
            );
        config
            .ThrowInterface.Should()
            .Equal(
                feed.ThrowTimeInterfaceFor("B1"),
                "factory should lock throw-time interface for the fedee"
            );
    }

    [Test]
    public void Wizard_Generation_Still_Streams_Results_Incrementally()
    {
        // Finding #7: WizardPage now buffers full SiteswapListGeneration list (streaming regression).
        var wizardSource = ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor.cs")
        );

        wizardSource
            .Should()
            .Contain(
                "await foreach",
                "Wizard must stream siteswaps (await foreach) rather than only await a full list"
            );
        wizardSource
            .Should()
            .MatchRegex(
                @"State\.Results\.Add(?:Range)?\([\s\S]{0,200}?StateHasChanged",
                "Wizard must push partial results to UI during generation"
            );
    }

    private static bool IsFeedToGenerationWorkflowConfigFactory(MethodInfo method)
    {
        if (method.ReturnType != typeof(GenerationWorkflowConfig))
        {
            return false;
        }

        var name = method.Name;
        if (
            name.Contains("GenerationWorkflowConfig", StringComparison.Ordinal)
            || name.Contains("ToGenerationWorkflow", StringComparison.Ordinal)
            || name.Contains("FromFeed", StringComparison.Ordinal)
            || name is "Create" or "From"
        )
        {
            var parameters = method.GetParameters();
            return parameters.Any(p =>
                p.ParameterType == typeof(NormalFeedSession)
                || p.ParameterType == typeof(string)
                || p.Name is "role" or "fedee"
            );
        }

        return false;
    }

    private static GenerationWorkflowConfig InvokeFeedConfigFactory(
        MethodInfo factory,
        NormalFeedSession feed,
        string role
    )
    {
        var parameters = factory.GetParameters();
        object?[] args;
        if (factory.IsStatic)
        {
            args = parameters.Length switch
            {
                1 when parameters[0].ParameterType == typeof(NormalFeedSession) => [feed],
                2 => parameters[0].ParameterType == typeof(NormalFeedSession)
                    ? [feed, role]
                    : [role, feed],
                _ => throw new AssertionException($"Unexpected factory signature: {factory}"),
            };
            return (GenerationWorkflowConfig)factory.Invoke(null, args)!;
        }

        args = parameters.Length switch
        {
            0 => [],
            1 when parameters[0].ParameterType == typeof(string) => [role],
            _ => throw new AssertionException($"Unexpected instance factory signature: {factory}"),
        };
        return (GenerationWorkflowConfig)factory.Invoke(feed, args)!;
    }

    internal static string ReadGeneratorSource(string relativePathUnderGeneratorProject)
    {
        return File.ReadAllText(
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..",
                "..",
                "..",
                "..",
                "Siteswaps.Generator",
                relativePathUnderGeneratorProject
            )
        );
    }
}

[TestFixture]
public class ConfiguredGenerationWorkflowApiReproTests
{
    [Test]
    public void Equal_Config_Value_Does_Not_Reset_Session_On_ParametersSet()
    {
        // Finding #3: OnParametersSet uses ReferenceEquals → new equal Config wipes edits.
        var host = new ConfiguredGenerationWorkflow();
        SetParameter(
            host,
            nameof(ConfiguredGenerationWorkflow.Config),
            new GenerationWorkflowConfig { Period = 5, NumberOfJugglers = 2 }
        );
        InvokeOnParametersSet(host);
        host.Session.SetClubs(new Between { MinNumber = 4, MaxNumber = 6 });
        var clubsBefore = host.Session.State.Clubs;

        SetParameter(
            host,
            nameof(ConfiguredGenerationWorkflow.Config),
            new GenerationWorkflowConfig { Period = 5, NumberOfJugglers = 2 }
        );
        InvokeOnParametersSet(host);

        host.Session.State.Clubs.Should()
            .Be(clubsBefore, "value-equal Config must not recreate the session");
    }

    [Test]
    public void Host_GenerateAsync_Accepts_CancellationToken()
    {
        // Finding #4: host GenerateAsync has no cancel token; OnCancelled never wired.
        var generate = typeof(ConfiguredGenerationWorkflow).GetMethod(
            nameof(ConfiguredGenerationWorkflow.GenerateAsync),
            BindingFlags.Instance | BindingFlags.Public
        );

        generate.Should().NotBeNull();
        generate!
            .GetParameters()
            .Should()
            .Contain(
                p => p.ParameterType == typeof(CancellationToken),
                "ConfiguredGenerationWorkflow.GenerateAsync must accept CancellationToken"
            );
    }

    [Test]
    public void Host_Wires_OnCancelled_On_Cancellation()
    {
        // Finding #4: OnCancelled is declared but never invoked from GenerateAsync.
        var source = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine(
                "Components",
                "GenerationWorkflow",
                "ConfiguredGenerationWorkflow.razor.cs"
            )
        );

        source
            .Should()
            .Contain(
                "OnCancelled.InvokeAsync",
                "ConfiguredGenerationWorkflow must fire OnCancelled when generation is cancelled"
            );
        source
            .Should()
            .MatchRegex(
                @"GenerateAsync\s*\(\s*CancellationToken",
                "GenerateAsync must accept CancellationToken to support cancel → OnCancelled"
            );
    }

    [Test]
    public void ConfiguredGenerationWorkflow_Razor_Renders_Controls_Or_Wizard_Embeds_Host()
    {
        // Finding #8: razor is empty; Wizard does not host ConfiguredGenerationWorkflow.
        var hostRazor = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine("Components", "GenerationWorkflow", "ConfiguredGenerationWorkflow.razor")
        );
        var wizardMarkup = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine("Components", "WizardPage", "WizardPage.razor")
        );

        var hostRendersControls =
            hostRazor.Contains("IsPeriodVisible", StringComparison.Ordinal)
            || hostRazor.Contains("PeriodStepper", StringComparison.Ordinal)
            || hostRazor.Contains("JugglerPicker", StringComparison.Ordinal)
            || hostRazor.Contains("DualRangeSlider", StringComparison.Ordinal)
            || hostRazor.Contains("ThrowsChipGrid", StringComparison.Ordinal)
            || hostRazor.Contains("FilterList", StringComparison.Ordinal);
        var wizardEmbedsHost = wizardMarkup.Contains(
            "ConfiguredGenerationWorkflow",
            StringComparison.Ordinal
        );

        (hostRendersControls || wizardEmbedsHost)
            .Should()
            .BeTrue(
                "Plan: ConfiguredGenerationWorkflow.razor renders controls, or WizardPage embeds <ConfiguredGenerationWorkflow>"
            );
    }

    [Test]
    public void Visibility_Flags_Are_Bound_In_ConfiguredGenerationWorkflow_Markup()
    {
        // Finding #9: IsPeriodVisible / IsJugglersVisible exist but do not drive UI.
        var hostRazor = GenerationWorkflowInvariantReproTests.ReadGeneratorSource(
            Path.Combine("Components", "GenerationWorkflow", "ConfiguredGenerationWorkflow.razor")
        );

        hostRazor
            .Should()
            .Contain(
                "IsPeriodVisible",
                "locked Period visibility must gate Period UI in the host markup"
            );
        hostRazor
            .Should()
            .Contain(
                "IsJugglersVisible",
                "locked jugglers visibility must gate jugglers UI in the host markup"
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
