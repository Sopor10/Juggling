using Siteswap.Details;

namespace Siteswaps.Test;

public class LocalSiteswapTest
{
    [Test]
    public void FromLocals_With_Incomplete_Jugglers_Should_Return_Error()
    {
        var global = new Siteswap.Details.Siteswap(5, 3, 1);
        var local0 = new LocalSiteswap(global, 0, 3);
        var local1 = new LocalSiteswap(global, 1, 3);

        var result = LocalSiteswap.FromLocals([local0, local1]);

        result.Should().Be().Error("Expected 3 local siteswaps, but got 2.");
    }

    [Test]
    public void FromLocals_With_Duplicate_Jugglers_Should_Return_Error()
    {
        var global = new Siteswap.Details.Siteswap(5, 3, 1);
        var local0 = new LocalSiteswap(global, 0, 3);
        var local1 = new LocalSiteswap(global, 1, 3);
        var local2 = new LocalSiteswap(global, 1, 3); // Duplicate index 1

        var result = LocalSiteswap.FromLocals([local0, local1, local2]);

        result.Should().Be().Error("Juggler indices are not unique or incomplete.");
    }

    [Test]
    public void FromLocals_With_Unordered_Input_Should_Work()
    {
        var global = new Siteswap.Details.Siteswap(5, 3, 1);
        var local0 = new LocalSiteswap(global, 0, 3);
        var local1 = new LocalSiteswap(global, 1, 3);
        var local2 = new LocalSiteswap(global, 2, 3);

        var result = LocalSiteswap.FromLocals([local2, local0, local1]);

        result.Should().Be().Siteswap(global);
    }

    [Test]
    public void FromLocals_Should_Reconstruct_Siteswap()
    {
        var global = new Siteswap.Details.Siteswap(5, 3, 1); // 3 Jongleure: 5, 3, 1
        var local0 = new LocalSiteswap(global, 0, 3);
        var local1 = new LocalSiteswap(global, 1, 3);
        var local2 = new LocalSiteswap(global, 2, 3);

        var result = LocalSiteswap.FromLocals([local0, local1, local2]);

        result.Should().Be().Siteswap(global);
    }

    [Test]
    public void Combining_Two_Period_5_Siteswaps_Results_In_Period_10()
    {
        var local0 = new LocalSiteswap(new("95894"), 0, 2);
        var local1 = new LocalSiteswap(new("92897"), 1, 2);

        var result = LocalSiteswap.FromLocals([local0, local1]);

        result.Should().Be().Siteswap(new Siteswap.Details.Siteswap("9289495897"));
    }
}
