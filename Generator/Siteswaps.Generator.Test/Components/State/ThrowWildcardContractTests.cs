using FluentAssertions;
using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Test.Components.State;

/// <summary>Contract tests for Throw.AnyPass (-2) / AnySelf (-3) Core wildcard sentinels.</summary>
[TestFixture]
public class ThrowWildcardContractTests
{
    [Test]
    public void AnyPass_And_AnySelf_Use_Core_Wildcard_Sentinel_Heights()
    {
        // Must stay aligned with FlexiblePatternFilter / RotationAwareFlexiblePatternFilter.
        Throw.AnyPass.Height.Should().Be(-2);
        Throw.AnySelf.Height.Should().Be(-3);
        Throw.AnyPass.DisplayValue.Should().Be("P");
        Throw.AnySelf.DisplayValue.Should().Be("S");
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void AnyPass_GetHeightForJugglers_Preserves_Pass_Sentinel_For_Any_Juggler_Count(
        int jugglers
    )
    {
        // Desired: wildcards stay sentinels; today Height*jugglers/2 corrupts them for jugglers != 2.
        Throw.AnyPass.GetHeightForJugglers(jugglers, useLiteralValue: false).Should().Equal(-2);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void AnySelf_GetHeightForJugglers_Preserves_Self_Sentinel_For_Any_Juggler_Count(
        int jugglers
    )
    {
        Throw.AnySelf.GetHeightForJugglers(jugglers, useLiteralValue: false).Should().Equal(-3);
    }
}
