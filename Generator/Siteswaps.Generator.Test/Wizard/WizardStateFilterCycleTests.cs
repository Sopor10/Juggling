using FluentAssertions;
using Siteswaps.Generator.Components.Internal.EasyFilter;
using Siteswaps.Generator.Core.Generator.Filter;

namespace Siteswaps.Generator.Test.Wizard;

[TestFixture]
public class WizardStateFilterCycleTests
{
    /// <summary>Summary: A don't-care state cycles to occupied.</summary>
    [Test]
    public void Cycle_DontCare_To_Occupied()
    {
        EasyStateFilter.Cycle(StateValue.DontCare).Should().Be(StateValue.Occupied);
    }

    /// <summary>Summary: An occupied state cycles to free.</summary>
    [Test]
    public void Cycle_Occupied_To_Free()
    {
        EasyStateFilter.Cycle(StateValue.Occupied).Should().Be(StateValue.Free);
    }

    /// <summary>Summary: A free state cycles back to don't-care.</summary>
    [Test]
    public void Cycle_Free_To_DontCare()
    {
        EasyStateFilter.Cycle(StateValue.Free).Should().Be(StateValue.DontCare);
    }
}
