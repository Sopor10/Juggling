using FluentAssertions;
using Siteswaps.Generator.Components.Internal.EasyFilter;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.Wizard;

[TestFixture]
public class WizardGenerationCacheKeyTests
{
    /// <summary>Summary: Same generator inputs must yield the identical localStorage cache key.</summary>
    [Test]
    public void From_Same_Inputs_Yields_Same_Key()
    {
        var a = CreateDefaultState();
        var b = CreateDefaultState();

        WizardGenerationCacheKey.From(a).Should().Be(WizardGenerationCacheKey.From(b));
    }

    /// <summary>Summary: Throw selection order must not change the cache key.</summary>
    [Test]
    public void From_Throw_Order_Does_Not_Affect_Key()
    {
        var a = CreateDefaultState();
        var b = CreateDefaultState();
        b.AllowedThrows.Clear();
        b.AllowedThrows.AddRange(a.AllowedThrows.AsEnumerable().Reverse());

        WizardGenerationCacheKey.From(a).Should().Be(WizardGenerationCacheKey.From(b));
    }

    /// <summary>Summary: Different period must produce a different cache key.</summary>
    [Test]
    public void From_Different_Period_Changes_Key()
    {
        var a = CreateDefaultState();
        var b = CreateDefaultState();
        b.Period = new Period(7);

        WizardGenerationCacheKey.From(a).Should().NotBe(WizardGenerationCacheKey.From(b));
    }

    /// <summary>Summary: Filters in the nested tree are part of the serialized generator input key.</summary>
    [Test]
    public void From_Includes_Filters()
    {
        var a = CreateDefaultState();
        var b = CreateDefaultState();
        b.FilterTree = new FilterTree(
            new FilterLeaf(
                new WizardIdentifiedFilter(
                    1,
                    new EasyNumberFilter.NumberFilter
                    {
                        Amount = 1,
                        Type = EasyNumberFilter.NumberFilterType.Exactly,
                        Throw = Throw.Zip,
                    }
                )
            )
        );

        WizardGenerationCacheKey.From(a).Should().NotBe(WizardGenerationCacheKey.From(b));
        WizardGenerationCacheKey.From(b).Should().Contain("Exactly");
    }

    private static WizardState CreateDefaultState() => new();
}
