using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Siteswaps.Generator.Api;

namespace Siteswaps.Generator.Components.Test;

public abstract class FluxorTestContext : BunitTestContext
{
    [SetUp]
    public void ExtraSetup()
    {
        TestContext
            .Services
            .AddSingleton(x => Mock.Of<ISiteswapGenerator>())
            .AddSingleton(x => Mock.Of<ISiteswapGeneratorFactory>())
            .AddSingleton(x => Mock.Of<IFilterBuilder>())
            .AddFluxor(options => options.ScanAssemblies(typeof(Generator.Components.AssemblyInfo).Assembly));
        RenderComponent<Fluxor.Blazor.Web.StoreInitializer>();
    }

    public IState<T> GetState<T>() => this.TestContext.Services.GetService<IState<T>>();
    
}