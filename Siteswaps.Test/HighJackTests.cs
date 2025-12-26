using Meziantou.Framework.InlineSnapshotTesting;

namespace Siteswaps.Test;

public class HighJackTests
{
    [Test]
    public void Period_3_HighJacks()
    {
        var sut = new Siteswap.Details.Siteswap(8, 5, 5);
        InlineSnapshot.Validate(
            string.Join(Environment.NewLine, sut.GetHighJacks().Select(x => x.ToString())),
            "588"
        );
    }

    [Test]
    public void TestMCode2()
    {
        var sut = new Siteswap.Details.Siteswap(8, 5, 5);
        var result = sut.PossibleTransitions(new Siteswap.Details.Siteswap(8, 5, 8, 2, 5, 8), 1);
        InlineSnapshot.Validate(
            string.Join(Environment.NewLine, result.Select(x => x.PrettyPrint())),
            "855 -8-> 858258"
        );
    }

    [Test]
    public void Swap_Test()
    {
        var sut = new Siteswap.Details.Siteswap(5, 1);
        var result = sut.Swap(0, 1);
        InlineSnapshot.Validate(result.ToString(), "24");
    }
}
