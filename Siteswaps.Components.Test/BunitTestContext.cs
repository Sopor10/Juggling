using Bunit;
using Fluxor;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Siteswaps.Components.Test;

public abstract class BunitTestContext : TestContextWrapper
{
    [SetUp]
    public void Setup()
    {
        TestContext = new TestContext();
        TestContext
            .Services
            .AddFluxor(options => options.ScanAssemblies(typeof(Components.Assembly).Assembly));
    }

    [TearDown]
    public void TearDown()
    {
        TestContext?.Dispose();
    }
}

public abstract class FluxorTestContext : BunitTestContext
{
    [SetUp]
    public void ExtraSetup()
    {
        TestContext
            .Services
            .AddFluxor(options => options.ScanAssemblies(typeof(Components.Assembly).Assembly));
        RenderComponent<Fluxor.Blazor.Web.StoreInitializer>();
    }

    public IState<T> GetState<T>() => this.TestContext.Services.GetService<IState<T>>();

}