using System.Runtime.CompilerServices;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.GenerationWorkflow;

/// <summary>
/// Shared Siteswap list generation used by the wizard and other hosts (e.g. feeding).
/// Supports full-list and streaming generation — no selection UI.
/// </summary>
public static class SiteswapListGeneration
{
    public static async Task<IReadOnlyList<Siteswap>> GenerateAsync(
        WizardState state,
        CancellationToken cancellationToken = default
    )
    {
        var results = new List<Siteswap>();
        await foreach (var siteswap in GenerateStreamAsync(state, cancellationToken))
        {
            results.Add(siteswap);
        }

        return results;
    }

    public static async IAsyncEnumerable<Siteswap> GenerateStreamAsync(
        WizardState state,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var generators = FilterTranslation.CreateGenerators(state);

        foreach (var generator in generators)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await foreach (var siteswap in generator.GenerateAsync(cancellationToken))
            {
                yield return siteswap;
            }
        }
    }
}
