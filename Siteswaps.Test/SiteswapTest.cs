using FluentAssertions;

namespace Siteswaps.Test;

public class SiteswapTest
{
    [Test]
    public void UniqueRepresentationOfSiteswap()
    {
        var s1 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[]{4, 4, 1});
        var s2 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[]{4, 1, 4});
        s1.EnumerateValues(1).Should().Equal(s2.EnumerateValues(1));
    }
        
    [Test]
    public void UniqueRepresentationOfSiteswap2()
    {
        var s1 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[]{13, 8,13,9,3,6,4});
        var s2 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[]{13,9,3,6,4,13,8});

        s1.EnumerateValues(1).Should().Equal(s2.EnumerateValues(1));
    }
    
    
    [Test]
    public void DifferentUniqueRepresentation()
    {
        var s1 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[]{3,1,5});

        s1.EnumerateValues(1).Should().NotEqual(new[]{3,1,5});
    }
    
    [Test]
    public void UniqueRepresentationOfSiteswapShouldWork()
    {
        var s1 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[]{13, 8,13,9,3,6,4});

        var enumerateValues = s1.EnumerateValues(1).ToList();
        enumerateValues.First().Should().Be(13);
        enumerateValues.Skip(1).First().Should().Be(9);
    }
    
    [Test]
    [TestCase(4,4,1)]
    [TestCase(5,3,1)]
    [TestCase(1)]
    [TestCase(9,7,5,3,1)]
    [TestCase(7,5,6,6)]
    public void ValidSiteswaps(params int[] siteswap)
    {
        var result = Siteswap.Details.Siteswap.TryCreate(siteswap, out var sut);

        result.Should().BeTrue();
    }
        
    [Test]
    [TestCase(4,3)]
    [TestCase(2,1)]
    public void InvalidSideswaps(params int[] siteswap)
    {
        var result = Siteswap.Details.Siteswap.TryCreate(siteswap, out var sut);

        result.Should().BeFalse();
    }
}