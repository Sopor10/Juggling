using System.Runtime.CompilerServices;
using Siteswap.Details;
using Siteswap.Details.CausalDiagram;

namespace Siteswaps.Test.CausalDiagram;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyImageMagick.Initialize();
        VerifyImageMagick.RegisterComparers();
        VerifyImageMagick.RegisterComparers(0.05);
    }
}

[TestFixture]
public class CausalDiagramRendererTests
{
    [Test]
    public async Task Render_531_CausalDiagram()
    {
        var causalDiagram = new CausalDiagramGenerator().Generate(
            new Siteswap.Details.Siteswap(5, 3, 1),
            HandsFor4HandedSiteswap()
        );

        await Verify(causalDiagram);
    }

    private static CyclicArray<Hand> HandsFor4HandedSiteswap()
    {
        return new CyclicArray<Hand>(
            new Hand("R", new Person("A")),
            new Hand("R", new Person("B")),
            new Hand("L", new Person("A")),
            new Hand("L", new Person("B"))
        );
    }
}
