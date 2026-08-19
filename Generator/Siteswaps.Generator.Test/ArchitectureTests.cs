using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;
using ArchunitNetExtension;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Siteswaps.Generator.Test;

public class ArchitectureTests
{
    private readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Siteswaps.Generator.AssemblyInfo).Assembly,
            typeof(Siteswaps.Generator.Core.Generator.SiteswapGenerator).Assembly
        )
        .Build();

    [Test]
    public void Generator_Should_Not_Depend_On_Components()
    {
        IArchRule rule = Types()
            .That()
            .ResideInNamespace("Siteswaps.Generator.Core.Generator")
            .Should()
            .NotDependOnAny(Types().That().ResideInNamespace("Siteswaps.Generator.Components"));
        rule.Check(Architecture);
    }

    [Test]
    public void Generator_Namespaces_Should_Not_Form_Cycles()
    {
        IArchRule rule = new[]
        {
            Types().That().ResideInNamespace("Siteswaps.Generator.Core.Generator", true),
            Types().That().ResideInNamespace("Siteswaps.Generator.Components", true),
        }
            .AsSlices()
            .Should()
            .BeFreeOfCycles();

        rule.Check(Architecture);
    }
}
