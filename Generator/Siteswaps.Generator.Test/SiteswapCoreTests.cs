using FluentAssertions;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test;

public class SiteswapCoreTests
{
    [Test]
    public void String_Notation_Parses_Extended_Heights()
    {
        Siteswap.CreateFromCorrect("a72").Items.Should().Equal(10, 7, 2);
    }

    [Test]
    public void String_Notation_Renders_Extended_Heights()
    {
        Siteswap.CreateFromCorrect("a72").ToString().Should().Be("a72");
    }

    [Test]
    public void String_Notation_Exposes_The_Period()
    {
        Siteswap.CreateFromCorrect("a72").Period.Value.Should().Be(3);
    }

    [Test]
    public void Siteswaps_With_The_Same_Notation_Are_Equal()
    {
        var first = Siteswap.CreateFromCorrect(10, 7, 2);
        var same = Siteswap.CreateFromCorrect("a72");

        first.Equals(same).Should().BeTrue();
    }

    [Test]
    public void Siteswaps_With_Different_Notation_Are_Not_Equal()
    {
        var first = Siteswap.CreateFromCorrect(10, 7, 2);
        var different = Siteswap.CreateFromCorrect(10, 7, 3);

        first.Equals(different).Should().BeFalse();
    }

    [Test]
    public void Siteswap_Is_Not_Equal_To_Null()
    {
        Siteswap.CreateFromCorrect(10, 7, 2).Equals((Siteswap?)null).Should().BeFalse();
    }

    [Test]
    public void Siteswap_Exposes_The_Average()
    {
        Siteswap.CreateFromCorrect(5, 3, 1).Average.Should().BeApproximately(3, 0.001);
    }

    [Test]
    public void Siteswap_Recognizes_A_Valid_Siteswap()
    {
        Siteswap.CreateFromCorrect(5, 3, 1).IsValid().Should().BeTrue();
    }

    [Test]
    public void Siteswap_Rejects_Duplicate_Landing_Positions()
    {
        Siteswap.CreateFromCorrect(3, 2).IsValid().Should().BeFalse();
    }

    [Test]
    public void Siteswap_Rejects_Landing_Positions_That_Only_Collide_After_Modulo()
    {
        Siteswap.CreateFromCorrect(1, 3, 0).IsValid().Should().BeFalse();
    }

    [Test]
    public void LocalSiteswap_Exposes_Global_Notation()
    {
        var local = Siteswap.CreateFromCorrect(5, 3, 1).GetLocalSiteswap(0, 2);

        local.GlobalNotation.Should().Be("513");
    }

    [Test]
    public void LocalSiteswap_Exposes_Local_Notation()
    {
        var local = Siteswap.CreateFromCorrect(5, 3, 1).GetLocalSiteswap(0, 2);

        local.LocalNotation.Should().Be("2.5 0.5 1.5");
    }

    [Test]
    public void LocalSiteswap_Exposes_The_Average()
    {
        Siteswap
            .CreateFromCorrect(5, 3, 1)
            .GetLocalSiteswap(0, 2)
            .Average()
            .Should()
            .BeApproximately(1.5, 0.001);
    }

    [Test]
    public void LocalSiteswap_Rejects_An_Invalid_Global_Siteswap()
    {
        Siteswap
            .CreateFromCorrect(5, 3, 1)
            .GetLocalSiteswap(0, 2)
            .IsValidAsGlobalSiteswap()
            .Should()
            .BeFalse();
    }

    [Test]
    public void LocalSiteswap_Recognizes_A_Valid_Global_Siteswap()
    {
        Siteswap
            .CreateFromCorrect(5, 3, 1)
            .GetLocalSiteswap(0, 1)
            .IsValidAsGlobalSiteswap()
            .Should()
            .BeTrue();
    }
}
