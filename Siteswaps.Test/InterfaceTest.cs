using FluentAssertions;
using Siteswap.Details;

namespace Siteswaps.Test;

public class InterfaceTest
{
    [Test]
    public void InterfaceOf53Is35()
    {
        Siteswap.Details.Siteswap.TryCreate("53", out var siteswap).Should().BeTrue();
        siteswap!.Interface.Items.Should().Equal(3, 5);
    }
}
