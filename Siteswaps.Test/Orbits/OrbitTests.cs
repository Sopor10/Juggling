using Siteswap.Details;

namespace Siteswaps.Test.Orbits;

public class OrbitTests
{
    [Test]
    [TestCase(5, 3, 1)]
    [TestCase(4, 4, 1)]
    [TestCase(5, 1)]
    [TestCase(3)]
    public async Task CanCalculateOrbit(params int[] input)
    {
        var siteswap = new Siteswap.Details.Siteswap(input);
        var result = siteswap.GetOrbits();

        await Verify(new OrbitPrinter(siteswap).Print(result));
    }

    private class OrbitPrinter(Siteswap.Details.Siteswap siteswap)
    {
        public string Print(List<Orbit> orbits)
        {
            return $"Siteswap: {siteswap}\nOrbits:\n" + string.Join("\n", orbits.Select(Print));
        }

        private string Print(Orbit orbit)
        {
            return orbit.Items.ToSiteswapString();
        }
    }
}
