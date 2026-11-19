using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Siteswap.Details;
using Siteswap.Details.CausalDiagram;
using Siteswap.Details.Jif;

namespace Siteswaps.Test.Jif;

[TestFixture]
public class JifMapperTests
{
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
    {
        WriteIndented = true,
    };

    // Props counts and person counts are taken from the Passist dev pages for each route.
    [TestCase("531", 1, 3)]
    [TestCase("441", 1, 3)]
    [TestCase("744", 1, 5)]
    [TestCase("97531", 1, 5)]
    [TestCase("756", 1, 6)]
    [TestCase("966", 1, 7)]
    [TestCase("96686", 2, 7)]
    [TestCase("97531", 2, 5)]
    [TestCase("888", 2, 8)]
    [TestCase("531", 3, 3)]
    [TestCase("777", 3, 7)]
    [TestCase("999", 3, 9)]
    public void Maps_Passist_Siteswap_Cases(string notation, int jugglers, int props)
    {
        var siteswap = new global::Siteswap.Details.Siteswap(
            notation.Select(character => character - '0').ToArray()
        );

        var actual = JifMapper.Map(siteswap, jugglers);

        actual.Valid.Should().BeTrue();
        actual.HighLevelDescription!.Description.Should().Be(notation);
        actual.Jugglers.Should().HaveCount(jugglers);
        actual.Limbs.Should().HaveCount(jugglers * 2);
        actual.Props.Should().HaveCount(props);
        actual.Throws.Should().NotBeEmpty();
        actual.Repetition!.Period.Should().Be(actual.Throws!.Count);
        actual
            .Throws.Should()
            .OnlyContain(throwEvent =>
                throwEvent.Duration > 0
                && throwEvent.From >= 0
                && throwEvent.From < actual.Limbs!.Count
                && throwEvent.To >= 0
                && throwEvent.To < actual.Limbs.Count
            );
    }

    [Test]
    public async Task Maps_531_To_The_Passist_Example()
    {
        var actual = JifMapper.Map(
            new global::Siteswap.Details.Siteswap(5, 3, 1),
            numberOfJugglers: 1
        );
        var actualJson = JsonSerializer.Serialize(actual, SnapshotJsonOptions);

        await Verify(actualJson);
    }

    [Test]
    public async Task Maps_Siteswap_Lab_Three_Person_Pattern_To_Jif()
    {
        IReadOnlyList<int> timeZones = [0, 1, 1];
        IReadOnlyList<IReadOnlyList<int>> heights =
        [
            new[] { 9, 9, 6, 6, 6 },
            new[] { 6, 6, 3, 6, 6 },
            new[] { 6, 3, 6, 6, 6 },
        ];
        IReadOnlyList<IReadOnlyList<int>> targets =
        [
            new[] { 2, 1, 0, 0, 0 },
            new[] { 1, 1, 0, 1, 1 },
            new[] { 2, 0, 2, 2, 2 },
        ];

        var causalDiagram = PassingDiagramBuilder.Build(timeZones, heights, targets);
        var actual = JifMapper.Map(
            causalDiagram,
            name: "Siteswap Lab passing pattern",
            description: "3 jugglers, period 5"
        );

        actual.Jugglers.Should().HaveCount(3);
        actual.Limbs.Should().HaveCount(6);
        actual.Throws.Should().HaveCount(270);
        actual.Repetition!.Period.Should().Be(180);
        actual.Repetition.LimbPermutation.Should().Equal(0, 1, 2, 3, 4, 5);
        actual.Repetition.PropPermutation.Should().Equal(Enumerable.Range(0, 9));
        actual.Throws.Should().OnlyContain(throwEvent => throwEvent.Prop != null);
        actual
            .Throws!.Should()
            .ContainSingle(throwEvent => throwEvent.Time == 0 && throwEvent.From == 0)
            .Which.Should()
            .BeEquivalentTo(
                new JifThrow
                {
                    Time = 0,
                    Duration = 9,
                    From = 0,
                    To = 4,
                    Label = "9",
                    Prop = 0,
                }
            );
        actual
            .Throws.Should()
            .ContainSingle(throwEvent => throwEvent.Time == 1 && throwEvent.From == 2)
            .Which.Should()
            .BeEquivalentTo(
                new JifThrow
                {
                    Time = 1,
                    Duration = 6,
                    From = 2,
                    To = 3,
                    Label = "6",
                    Prop = 1,
                }
            );

        await Verify(JsonSerializer.Serialize(actual, SnapshotJsonOptions));
    }

    [Test]
    public void Maps_Passist_Two_Jugglers_To_Radial_Poses_And_Center_Facing()
    {
        var actual = JifMapper.Map(
            new global::Siteswap.Details.Siteswap(9, 6, 6, 8, 6),
            numberOfJugglers: 2
        );

        actual
            .Jugglers.Should()
            .BeEquivalentTo([
                new JifJuggler
                {
                    Name = "A",
                    Position = [1.6m, 0, 0],
                    LookAt = [0, 0, 0],
                },
                new JifJuggler
                {
                    Name = "B",
                    Position = [-1.6m, 0, 0],
                    LookAt = [0, 0, 0],
                },
            ]);
    }

    [Test]
    public void Maps_Passist_Three_Jugglers_To_Radial_Poses_And_Center_Facing()
    {
        var actual = JifMapper.Map(
            new global::Siteswap.Details.Siteswap(9, 6, 6, 8, 6),
            numberOfJugglers: 3
        );

        actual
            .Jugglers.Should()
            .BeEquivalentTo([
                new JifJuggler
                {
                    Name = "A",
                    Position = [1.8m, 0, 0],
                    LookAt = [0, 0, 0],
                },
                new JifJuggler
                {
                    Name = "B",
                    Position = [-0.9m, 0, 1.559m],
                    LookAt = [0, 0, 0],
                },
                new JifJuggler
                {
                    Name = "C",
                    Position = [-0.9m, 0, -1.559m],
                    LookAt = [0, 0, 0],
                },
            ]);
    }
}
