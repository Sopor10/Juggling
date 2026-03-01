using System.Collections.Immutable;
using FluentAssertions;
using Siteswap.Details;
using Siteswap.Details.CausalDiagram;
using Hand = Siteswap.Details.CausalDiagram.Hand;

namespace Siteswaps.Test.CausalDiagram;

public class CausalDiagramGeneratorTest
{
    [Test]
    public void METHOD()
    {
        var sut = new CausalDiagramGenerator();
        var hands = new List<Hand>()
        {
            new("R", new Person("A")),
            new("L", new Person("A")),
        }.ToImmutableList();
        var result = sut.Generate(
            new Siteswap.Details.Siteswap(new[] { 4, 2, 3 }.ToCyclicArray()),
            hands.ToCyclicArray()
        );

        result.Throws.Should().HaveCount(6);
    }

    [Test]
    public void Diagram531()
    {
        var sut = new CausalDiagramGenerator();
        var hands = new List<Hand>()
        {
            new("R", new Person("A")),
            new("R", new Person("B")),
            new("L", new Person("A")),
            new("L", new Person("B")),
        }.ToImmutableList();
        var result = sut.Generate(
            new Siteswap.Details.Siteswap(new[] { 5, 3, 1 }.ToCyclicArray()),
            hands.ToCyclicArray()
        );

        result.Throws.Should().HaveCount(12);

        result.Transitions.Should().HaveCount(11);
    }
}
