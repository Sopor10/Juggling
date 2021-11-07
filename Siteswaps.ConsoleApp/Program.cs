using System.IO;
using DotNetGraph.Extensions;
using Siteswaps.StateDiagram;
using Siteswaps.Visualization;

var input = new StateGraphGeneratorInput(3, 4);
var graph = new StateGraphGenerator().Generate(input);
var dot = new GraphFactory()
    .Create(graph)
    .Compile(true);
var title = $"Period-{input.NumberOfObjects}_NumberOfObjects-{input.Period}.dot";
File.WriteAllText(title, dot);
