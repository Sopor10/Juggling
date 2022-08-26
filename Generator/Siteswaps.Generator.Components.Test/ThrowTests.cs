using FluentAssertions;
using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Components.Test;

public class ThrowTests
{
    [Test]
    public void Heff_For_3_People()
    {
        Throw.Heff.GetHeightForJugglers(3).Should().BeEquivalentTo(new[] { 12});
    }
    
    [Test]
    public void Pass_For_3_People()
    {
        Throw.SinglePass.GetHeightForJugglers(3).Should().BeEquivalentTo(new[] { 10,11});
    }
    
    [Test]
    public void DoublePass_For_4_People()
    {
        Throw.DoublePass.GetHeightForJugglers(4).Should().BeEquivalentTo(new[] { 17,18,19});
    }
}