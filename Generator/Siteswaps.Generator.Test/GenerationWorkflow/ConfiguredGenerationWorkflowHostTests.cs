using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Siteswaps.Generator.Components.GenerationWorkflow;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.GenerationWorkflow;

[TestFixture]
public class ConfiguredGenerationWorkflowHostTests
{
    [Test]
    public void Host_Component_Exposes_Results_Callback_And_No_Selection_Api()
    {
        var type = typeof(ConfiguredGenerationWorkflow);

        type.GetProperty("OnResultsReady").Should().NotBeNull();
        type.GetProperty("OnCancelled").Should().NotBeNull();
        type.GetProperty("Config").Should().NotBeNull();
        type.GetMethod("SelectSiteswap").Should().BeNull();
        type.GetProperty("SelectedSiteswap").Should().BeNull();
        type.GetMethod("GenerateAsync").Should().NotBeNull();
    }

    [Test]
    public async Task Host_GenerateAsync_Delivers_List_Through_OnResultsReady()
    {
        // Finding #10: previous host test called Session.GenerateAsync, not the component.
        var host = new ConfiguredGenerationWorkflow();
        SetParameter(
            host,
            nameof(ConfiguredGenerationWorkflow.Config),
            new GenerationWorkflowConfig { Period = 3, NumberOfJugglers = 2 }
        );
        InvokeOnParametersSet(host);
        host.Session.SetClubs(new Between { MinNumber = 6, MaxNumber = 6 });

        IReadOnlyList<Siteswap>? delivered = null;
        SetParameter(
            host,
            nameof(ConfiguredGenerationWorkflow.OnResultsReady),
            new EventCallback<IReadOnlyList<Siteswap>>(
                null,
                (Func<IReadOnlyList<Siteswap>, Task>)(
                    results =>
                    {
                        delivered = results;
                        return Task.CompletedTask;
                    }
                )
            )
        );

        await host.GenerateAsync();

        delivered.Should().NotBeNull();
        delivered.Should().NotBeEmpty();
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
