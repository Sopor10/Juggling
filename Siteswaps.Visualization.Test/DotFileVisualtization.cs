using System.Threading.Tasks;
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
        var dot = new GraphFactory()
            .Create(graph)
            .Compile(true);

        await Verifier.Verify(dot);
    }
}