using System.Text;
using FluentAssertions;
using Meziantou.Framework.InlineSnapshotTesting;
using Siteswap.Details;
using Siteswap.Details.StateDiagram;
using Siteswap.Details.StateDiagram.Graph;

namespace Siteswaps.Test;

public class SiteswapTest
{
    [Test]
    public void UniqueRepresentationOfSiteswap()
    {
        var s1 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[] { 4, 4, 1 });
        var s2 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[] { 4, 1, 4 });
        s1.EnumerateValues(1).Should().Equal(s2.EnumerateValues(1));
    }

    [Test]
    public void UniqueRepresentationOfSiteswap2()
    {
        var s1 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[] { 13, 8, 13, 9, 3, 6, 4 });
        var s2 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[] { 13, 9, 3, 6, 4, 13, 8 });

        s1.EnumerateValues(1).Should().Equal(s2.EnumerateValues(1));
    }

    [Test]
    public void DifferentUniqueRepresentation()
    {
        var s1 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[] { 3, 1, 5 });

        s1.EnumerateValues(1).Should().NotEqual(new[] { 3, 1, 5 });
    }

    [Test]
    public void UniqueRepresentationOfSiteswapShouldWork()
    {
        var s1 = Siteswap.Details.Siteswap.ToUniqueRepresentation(new[] { 13, 8, 13, 9, 3, 6, 4 });

        var enumerateValues = s1.EnumerateValues(1).ToList();
        enumerateValues.First().Should().Be(13);
        enumerateValues.Skip(1).First().Should().Be(9);
    }

    [Test]
    [TestCase(4, 4, 1)]
    [TestCase(5, 3, 1)]
    [TestCase(1)]
    [TestCase(9, 7, 5, 3, 1)]
    [TestCase(7, 5, 6, 6)]
    public void ValidSiteswaps(params int[] siteswap)
    {
        var result = Siteswap.Details.Siteswap.TryCreate(siteswap, out var sut);

        result.Should().BeTrue();
    }

    [Test]
    [TestCase(4, 3)]
    [TestCase(2, 1)]
    public void InvalidSideswaps(params int[] siteswap)
    {
        var result = Siteswap.Details.Siteswap.TryCreate(siteswap, out var sut);

        result.Should().BeFalse();
    }

    [Test]
    [TestCase("441")]
    [TestCase("531")]
    [TestCase("1")]
    [TestCase("97531")]
    [TestCase("7566")]
    [TestCase("aaa00")]
    public void ValidSiteswaps(string siteswap)
    {
        var result = Siteswap.Details.Siteswap.TryCreate(siteswap, out var sut);

        result.Should().BeTrue();
    }

    [Test]
    [TestCase("43")]
    [TestCase("21")]
    [TestCase("")]
    public void InvalidSideswaps(string siteswap)
    {
        var result = Siteswap.Details.Siteswap.TryCreate(siteswap, out var sut);

        result.Should().BeFalse();
    }

    [Test]
    public void Calculate_Transitions()
    {
        var sut = new Siteswap.Details.Siteswap(3);
        var to = new Siteswap.Details.Siteswap(5, 3, 1);
        var transitions = sut.PossibleTransitions(to, 3, 5);

        InlineSnapshot.Validate(PrettyPrint(transitions), "3 --> 531");
    }

    [Test]
    public void Create_Specific_Transition_Length_1()
    {
        var from = new Siteswap.Details.Siteswap(5, 3, 1);
        var to = new Siteswap.Details.Siteswap(4, 1, 4);
        var length = 1;

        var transitions = from.PossibleTransitions(to, length);

        InlineSnapshot.Validate(transitions.Single().PrettyPrint(), "531 -4-> 414");
    }

    private static string PrettyPrint(List<Transition> transitions)
    {
        var result = string.Join(
            Environment.NewLine,
            transitions.Select(x => x.PrettyPrint()).Order()
        );
        return result;
    }

    [Test]
    public void Create_Specific_Transition__Should_Find_All()
    {
        var from = new Siteswap.Details.Siteswap(5, 3, 1);
        var to = new Siteswap.Details.Siteswap(4, 1, 4);
        var length = 3;

        var transitions = from.PossibleTransitions(to, length);

        InlineSnapshot.Validate(
            PrettyPrint(transitions),
            """
            531 -4-> 414
            531 -52-> 414
            """
        );
    }

    [Test]
    public void State_Works()
    {
        var sut = new Siteswap.Details.Siteswap(0, 3, 0).State;
        InlineSnapshot.Validate(sut.ToString(), "01");
    }

    [Test]
    public void State_Should_Be_Able_To_Throw_A_0()
    {
        var sut = new Siteswap.Details.Siteswap(0, 3, 0).State;

        var state = sut.Advance().Throw(0);
        InlineSnapshot.Validate(state.ToString(), "1");
    }

    [Test]
    public void Transitions_Are_Real_Transitions()
    {
        var sut = new Siteswap.Details.Siteswap(4, 4, 1);
        var to = new Siteswap.Details.Siteswap(5, 1);
        var transitions = sut.PossibleTransitions(to, 3, 5);
        var result = string.Join(
            Environment.NewLine,
            transitions.Select(x => x.PrettyPrint()).Order()
        );
        InlineSnapshot.Validate(
            result,
            """
            441 -4-> 51
            441 -52-> 51
            """
        );
    }

    [Test]
    public async Task SiteswapList_Can_Create_Graph()
    {
        var sut = new SiteswapList(
            new Siteswap.Details.Siteswap(5, 3, 1),
            new Siteswap.Details.Siteswap(4, 4, 1),
            new Siteswap.Details.Siteswap(5, 1),
            new Siteswap.Details.Siteswap(5, 2, 2),
            new Siteswap.Details.Siteswap(6, 1, 1, 2, 5),
            new Siteswap.Details.Siteswap(5, 3, 0, 0, 2)
        );

        var result = sut.TransitionGraph(2);

        var s = PrettyPrint(result);
        await Verify(s);
    }

    [Test]
    public async Task SiteswapList_Can_Create_Graph_2()
    {
        var sut = new SiteswapList(
            new Siteswap.Details.Siteswap(5, 3, 1),
            new Siteswap.Details.Siteswap(5, 1)
        );

        var result = sut.TransitionGraph(2);

        var s = PrettyPrint(result);
        await Verify(s);
    }

    private string PrettyPrint(Graph<Siteswap.Details.Siteswap, Transition> result)
    {
        var builder = new StringBuilder();

        foreach (var s in result.Nodes.Select(x => x.ToString()).Order())
        {
            builder.AppendLine(s);
        }

        foreach (var edge in result.Edges.Select(x => x.Data.PrettyPrint()).Order())
        {
            builder.AppendLine(edge);
        }
        return builder.ToString();
    }

    [Test]
    public void ClubDistribution_ShouldCalculateGroundState()
    {
        var sut = new Siteswap.Details.Siteswap(7, 5, 6, 6);
        var distribution = sut.GetClubDistribution(2);

        // Bei 2 Jongleuren und 6 Bällen sollte jeder Jongleur 3 Bälle haben
        distribution.ToString().Should().NotBeEmpty();
    }

    [TestCase("86777", 2, "A: 2|2 B: 1|2")]
    [TestCase("441", 1, "A: 1|2")]
    [TestCase("531", 1, "A: 1|2")]
    [TestCase("51", 1, "A: 1|2")]
    [TestCase("27786", 2, "A: 1|2 B: 1|2")]
    [TestCase("7566", 2, "A: 1|2 B: 1|2")]
    [TestCase("966", 2, "A: 2|2 B: 1|2")]
    [TestCase("77772", 2, "A: 1|2 B: 1|2")]
    [TestCase("97531", 2, "A: 1|2 B: 1|1")]
    [TestCase("975", 2, "A: 2|2 B: 1|2")]
    [TestCase("96672", 2, "A: 1|2 B: 1|2")]
    public void ClubDistribution_Tests(string pattern, int numberOfJugglers, string expected)
    {
        Siteswap.Details.Siteswap.TryCreate(pattern, out var s);
        s.Should().NotBeNull();
        var distribution = s!.GetClubDistribution(numberOfJugglers);
        distribution.ToString().Should().Be(expected);
    }
}
