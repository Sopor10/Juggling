using System.Runtime.CompilerServices;
using Siteswap.Details;
using Siteswap.Details.CausalDiagram;
using SkiaSharp;
namespace Siteswaps.Test.CausalDiagram;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyImageMagick.Initialize();
        VerifyImageMagick.RegisterComparers();
    }
}



[TestFixture]
public class CausalDiagramRendererTests
{
    [Test]
    public async Task TestMethod()
    {
        var path = $"{Guid.NewGuid()}.png";

        var causalDiagram = new CausalDiagramGenerator()
            .Generate(new Siteswap.Details.CausalDiagram.Siteswap(5, 3, 1), HandsFor4HandedSiteswap());

        var bitmap = RenderToBitmap(causalDiagram);
        await SaveToFile(path, bitmap);
        await VerifyFile(path).UseExtension("png");
    }

    private static CyclicArray<Hand> HandsFor4HandedSiteswap()
    {
        return new CyclicArray<Hand>(
            new Hand("R", new Person("A")),
            new Hand("R", new Person("B")),
            new Hand("L", new Person("A")),
            new Hand("L", new Person("B")));
    }

    private static SKBitmap RenderToBitmap(Siteswap.Details.CausalDiagram.CausalDiagram causalDiagram)
    {
        var bitmap = new SKBitmap(500, 500);
        using var canvas = new SKCanvas(bitmap);
        new CausalDiagramRenderer().Render(canvas, causalDiagram);
        return bitmap;
    }

    private static async Task SaveToFile(string path, SKBitmap bitmap)
    {
        await using Stream s = File.OpenWrite(path);
        var data = SKImage.FromBitmap(bitmap).Encode(SKEncodedImageFormat.Png, 100);
        data.SaveTo(s);
    }
}