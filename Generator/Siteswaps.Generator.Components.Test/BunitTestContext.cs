using Bunit;
using TestContext = Bunit.TestContext;

namespace Siteswaps.Generator.Components.Test;

public abstract class BunitTestContext : TestContextWrapper
{
    [SetUp]
    public void Setup()
    {
        TestContext = new TestContext();
    }

    [TearDown]
    public void TearDown()
    {
        TestContext?.Dispose();
    }
}