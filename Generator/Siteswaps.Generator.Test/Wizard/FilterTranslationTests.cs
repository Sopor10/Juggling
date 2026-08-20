using System.Collections.Immutable;
using FluentAssertions;
using Siteswaps.Generator.Components.Internal.EasyFilter;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.Wizard;

[TestFixture]
public class FilterTranslationTests
{
    /// <summary>Summary: Nested Or inside And must still produce generators.</summary>
    [Test]
    public void CreateGenerators_Accepts_Nested_And_Or_Tree()
    {
        var state = new WizardState { NumberOfJugglers = 2 };
        state.Clubs = new Between { MinNumber = 6, MaxNumber = 6 };
        state.FilterTree = new FilterTree(
            new AndNode(
                ImmutableList.Create<FilterNode>(
                    Leaf(1, Number(2, Throw.Zip)),
                    new OrNode(
                        ImmutableList.Create<FilterNode>(
                            Leaf(2, Number(1, Throw.Heff)),
                            Leaf(3, Number(1, Throw.Self))
                        )
                    )
                )
            )
        );

        FilterTranslation.CreateGenerators(state).Should().NotBeEmpty();
    }

    /// <summary>Summary: Empty allowed throws must produce no generators.</summary>
    [Test]
    public void CreateGenerators_No_Throws_Returns_Empty()
    {
        var state = new WizardState();
        state.AllowedThrows.Clear();

        FilterTranslation.CreateGenerators(state).Should().BeEmpty();
    }

    /// <summary>
    /// Summary: Club range 5–7 must create one generator per count, including max.
    /// </summary>
    [Test]
    public void CreateGenerators_Club_Range_Includes_Max()
    {
        var state = new WizardState();
        state.Clubs = new Between { MinNumber = 5, MaxNumber = 7 };

        FilterTranslation
            .CreateGenerators(state)
            .Should()
            .HaveCount(
                3,
                because: "loop must be number <= MaxNumber so 5, 6, and 7 are all generated"
            );
    }

    /// <summary>Summary: Generated siteswaps must only contain heights from the selected throws.</summary>
    [Test]
    public void CreateGenerators_Results_Only_Use_Allowed_Heights()
    {
        var state = new WizardState { NumberOfJugglers = 2 };
        state.AllowedThrows.RemoveAll(t => t.Height == Throw.Zip.Height);
        state.Clubs = new Between { MinNumber = 6, MaxNumber = 6 };

        var allowedHeights = state
            .AllowedThrows.SelectMany(t => t.GetHeightForJugglers(state.NumberOfJugglers, false))
            .ToHashSet();
        var zipHeights = Throw.Zip.GetHeightForJugglers(state.NumberOfJugglers, false).ToHashSet();

        var found = CollectSiteswaps(state, limit: 40);

        found.Should().NotBeEmpty();
        foreach (var siteswap in found)
        {
            siteswap.Period.Value.Should().Be(5);
            ((int)siteswap.Average).Should().Be(6);
            siteswap.Items.Should().OnlyContain(h => allowedHeights.Contains(h));
            siteswap.Items.Should().NotContain(h => zipHeights.Contains(h));
        }
    }

    /// <summary>Summary: Exact number filter must constrain generated patterns accordingly.</summary>
    [Test]
    public void CreateGenerators_Exact_Number_Filter_Is_Applied()
    {
        var state = new WizardState { NumberOfJugglers = 2 };
        state.Clubs = new Between { MinNumber = 6, MaxNumber = 6 };
        state.FilterTree = new FilterTree(Leaf(1, Number(2, Throw.Heff)));

        var heffHeights = Throw
            .Heff.GetHeightForJugglers(state.NumberOfJugglers, false)
            .ToHashSet();
        var found = CollectSiteswaps(state, limit: 30);

        found.Should().NotBeEmpty();
        foreach (var siteswap in found)
        {
            siteswap.Items.Count(h => heffHeights.Contains(h)).Should().Be(2);
        }
    }

    /// <summary>
    /// Summary: Personalized number filter counts only throws by the selected juggler.
    /// </summary>
    [Test]
    public void CreateGenerators_Personalized_Number_Filter_Is_Applied()
    {
        const int jugglerIndex = 0;
        var state = new WizardState { NumberOfJugglers = 2 };
        state.Clubs = new Between { MinNumber = 6, MaxNumber = 6 };
        state.FilterTree = new FilterTree(Leaf(1, Number(2, Throw.Heff, jugglerIndex)));

        var heffHeights = Throw
            .Heff.GetHeightForJugglers(state.NumberOfJugglers, false)
            .ToHashSet();
        var found = CollectSiteswaps(state, limit: 30);

        found.Should().NotBeEmpty();
        foreach (var siteswap in found)
        {
            var fromJuggler = siteswap
                .Items.Where((_, i) => i % state.NumberOfJugglers == jugglerIndex)
                .Count(h => heffHeights.Contains(h));
            fromJuggler.Should().Be(2);
        }
    }

    /// <summary>Summary: Wrapping adjacent siblings must create a nested opposite-operator group.</summary>
    [Test]
    public void WrapAdjacentChildren_Creates_Nested_Group()
    {
        var a = Leaf(1, Number(1, Throw.Zip));
        var b = Leaf(2, Number(1, Throw.Heff));
        var c = Leaf(3, Number(1, Throw.Self));
        var root = new AndNode(ImmutableList.Create<FilterNode>(a, b, c));
        var tree = new FilterTree(root);

        var next = WizardFilterTree.WrapAdjacentChildren(tree, root, 0);

        next.Root.Should().BeOfType<AndNode>();
        var children = ((AndNode)next.Root!).Children;
        children.Should().HaveCount(2);
        children[0].Should().BeOfType<OrNode>();
        ((OrNode)children[0]).Children.Should().HaveCount(2);
        children[1].Should().Be(c);
    }

    private static List<Siteswap> CollectSiteswaps(WizardState state, int limit)
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

    private static FilterLeaf Leaf(int id, IFilterInformation filter) =>
        new(new WizardIdentifiedFilter(id, filter));

    private static EasyNumberFilter.NumberFilter Number(
        int amount,
        Throw t,
        int? jugglerIndex = null
    ) =>
        new()
        {
            Amount = amount,
            Type = EasyNumberFilter.NumberFilterType.Exactly,
            Throw = t,
            JugglerIndex = jugglerIndex,
        };
}
