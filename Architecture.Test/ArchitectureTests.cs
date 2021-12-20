using ArchUnitNET.Domain;
using NUnit.Framework;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent.Slices;
using ArchUnitNET.NUnit;
using Siteswaps;
using Siteswaps.Generator.Api.Filter;
using Siteswaps.Generator.Filter;

namespace Architecture.Test;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

public class ArchitectureTests
{
    private static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader().LoadAssemblies(typeof(Siteswap).Assembly)
            .Build();
    private readonly IObjectProvider<Class> SiteswapFilter =
        Classes().That().ImplementInterface(typeof(ISiteswapFilter));

    [Test]
    public void No_Dependencies_Between_High_Level_Folders()
    {
        SliceRuleDefinition.Slices().Matching("Siteswaps.(*)").Should().NotDependOnEachOther().Check(Architecture);
    }

    [Test]
    public void All_SiteswapFilter_Should_Be_Internal()
    {
        var filterAreInternal = Classes().That().Are(SiteswapFilter).Should().BeInternal();
        filterAreInternal.Check(Architecture);
    }
}