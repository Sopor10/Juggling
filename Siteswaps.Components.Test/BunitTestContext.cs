using Bunit;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Siteswaps.Generator;
using Siteswaps.Generator.Api;
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

public abstract class FluxorTestContext : BunitTestContext
{
    [SetUp]
    public void ExtraSetup()
    {
        TestContext
            .Services
            .AddSingleton(x => Mock.Of<ISiteswapGenerator>())
            .AddFluxor(options => options.ScanAssemblies(typeof(Components.Assembly).Assembly));
        RenderComponent<Fluxor.Blazor.Web.StoreInitializer>();
    }

    public IState<T> GetState<T>() => this.TestContext.Services.GetService<IState<T>>();
    
}