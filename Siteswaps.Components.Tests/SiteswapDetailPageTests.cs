namespace Siteswaps.Components.Tests;

using FluentAssertions;
using Generator.Generator;

public class SiteswapDetailPageTests
{
    [Test]
    public void Test1()
    {
        var localSiteswaps = Siteswap.CreateFromCorrect(7, 5, 6).GetLocalSiteswaps(2);
        
    }
}
