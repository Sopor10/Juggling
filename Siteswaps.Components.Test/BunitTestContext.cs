using Bunit;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Siteswaps.Components.Test;

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