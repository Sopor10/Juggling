using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Siteswaps.Generator.Test;

public class ArchitectureTests
{
    private readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(typeof(Siteswaps.Generator.AssemblyInfo).Assembly)
        .Build();

    [Test]
    public void Generator_Should_Not_Depend_On_Components()
    {
        IArchRule rule = Types()
            .That()
            .ResideInNamespaceMatching("Siteswaps.Generator.Generator")
            .Should()
            .NotDependOnAny(
                Types().That().ResideInNamespaceMatching("Siteswaps.Generator.Components")
            );
        rule.Check(Architecture);
    }
}
