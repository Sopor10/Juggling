using FluentAssertions;
using Siteswaps.Mcp.Server.Tools;

namespace Siteswaps.Mcp.Server.Test;

public class CombineLocalSiteswapsToolTests
{
    [Test]
    public void Combining_Two_Period_5_Siteswaps_Results_In_Period_10()
    {
        var tool = new CombineLocalSiteswapsTool();
        var result = tool.CombineLocalSiteswaps(["95894", "92897"]);

        result.IsSuccess.Should().BeTrue();
        result.Data!.GlobalSiteswap.Should().Be("9952889947");
    }

    [Test]
    public void CombineLocalSiteswaps_WithValidExample_ReturnsCorrectGlobal()
    {
        // 423 (global) with 3 jugglers:
        // Period is 3. N=3. Local period is 3/3 = 1.
        // Juggler 0 (index 0): siteswap[0 + 0*3] = 4. Local sequence: "4"
        // Juggler 1 (index 1): siteswap[1 + 0*3] = 2. Local sequence: "2"
        // Juggler 2 (index 2): siteswap[2 + 0*3] = 3. Local sequence: "3"

        var tool = new CombineLocalSiteswapsTool();
        var result = tool.CombineLocalSiteswaps(["4", "2", "3"]);

        result.IsSuccess.Should().BeTrue();
        result.Data!.GlobalSiteswap.Should().Be("423");
    }

    [Test]
    public void CombineLocalSiteswaps_WithPassingExample_ReturnsCorrectGlobal()
    {
        // Global: 771 (3 jugglers)
        // J0: 7
        // J1: 7
        // J2: 1
        var tool = new CombineLocalSiteswapsTool();
        var result = tool.CombineLocalSiteswaps(["7", "7", "1"]);

        if (!result.IsSuccess)
        {
            System.Console.WriteLine($"[DEBUG_LOG] Error: {result.Error!.Message}");
        }
        result.IsSuccess.Should().BeTrue();
        result.Data!.GlobalSiteswap.Should().Be("771");
    }

    [Test]
    public void CombineLocalSiteswaps_WithIncompatibleLengths_ReturnsError()
    {
        var tool = new CombineLocalSiteswapsTool();
        var result = tool.CombineLocalSiteswaps(["42", "3"]);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("different lengths");
    }
}
