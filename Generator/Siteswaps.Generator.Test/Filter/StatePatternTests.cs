using FluentAssertions;
using Siteswaps.Generator.Core.Generator;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Test.Filter;

[TestFixture]
public class StatePatternTests
{
    [Test]
    public void DontCare_Slots_Are_Ignored_When_Matching_A_State()
    {
        var pattern = new StatePattern([StateValue.Occupied, StateValue.DontCare, StateValue.Free]);

        pattern.Matches(new State(1, 1, 0)).Should().BeTrue();
    }

    [Test]
    public void Occupied_And_Free_Slots_Must_Match_Exactly()
    {
        var pattern = new StatePattern([StateValue.Occupied, StateValue.DontCare, StateValue.Free]);

        pattern.Matches(new State(0, 1, 0)).Should().BeFalse();
        pattern.Matches(new State(1, 1, 1)).Should().BeFalse();
    }

    [Test]
    public void Builder_Uses_DontCare_State_Patterns()
    {
        var pattern = new StatePattern([
            StateValue.DontCare,
            StateValue.DontCare,
            StateValue.Occupied,
        ]);
        var filter = new FilterBuilder(new SiteswapGeneratorInput(3, 2, 1, 3))
            .WithState(pattern)
            .Build();

        filter.CanFulfill(new PartialSiteswap([3])).Should().BeTrue();
    }
}
