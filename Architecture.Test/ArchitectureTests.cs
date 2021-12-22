using NUnit.Framework;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent.Slices;
using ArchUnitNET.NUnit;
using Siteswaps;

namespace Architecture.Test;

public class ArchitectureTests
{
    private static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader().LoadAssemblies(typeof(Siteswap).Assembly)
            .Build();

    [Test]
    public void No_Dependencies_Between_High_Level_Folders()
    {
        SliceRuleDefinition.Slices().Matching("Siteswaps.(*)").Should().NotDependOnEachOther().Check(Architecture);
    }
}