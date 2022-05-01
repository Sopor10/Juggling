using NUnit.Framework;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent.Slices;
using ArchUnitNET.NUnit;

namespace Architecture.Test;

public class ArchitectureTests
{
    private static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader().LoadAssemblies(typeof(Siteswap.Details.Siteswap).Assembly)
            .Build();

    [Test]
    public void No_Dependencies_Between_High_Level_Folders()
    {
        SliceRuleDefinition.Slices().Matching("Siteswap.Details.(*)").Should().NotDependOnEachOther().Check(Architecture);
    }
}