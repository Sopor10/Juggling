using System.IO;
using System.Threading.Tasks;
using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NUnit.Framework;
using Siteswap.Details.StateDiagram;
using VerifyNUnit;

namespace Siteswaps.Visualization.Test;

[TestFixture]
public class DotFileVisualtizationTest
{
    [Test]
    public async Task Verify_Dot_File_For_Simple_State_Graph()
    {
        var input = new StateGraphGeneratorInput(3, 5);
        var graph = new StateGraphGenerator().Generate(input);
        var dot = await new GraphFactory().Create(graph).Compile();

        await Verifier.Verify(dot);
    }
}

public static class DotgraphExtensions
{
    public static async Task<string> Compile(this DotGraph graph)
    {
        await using var writer = new StringWriter();
        var context = new CompilationContext(writer, new CompilationOptions() { Indented = true });
        await graph.CompileAsync(context);

        var result = writer.GetStringBuilder().ToString();
        return result;
    }
}
