using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Slices;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Siteswaps.Generator.Test;

public class ArchitectureTests
{
    private const string GeneratorNamespace = "Siteswaps.Generator.Core.Generator";
    private const string ComponentsNamespace = "Siteswaps.Generator.Components";

    private readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Siteswaps.Generator.AssemblyInfo).Assembly,
            typeof(Siteswaps.Generator.Core.Generator.SiteswapGenerator).Assembly
        )
        .Build();

    [Test]
    public void Generator_Should_Not_Depend_On_Components()
    {
        Types()
            .That()
            .ResideInNamespaceMatching($@"^{RegexEscape(GeneratorNamespace)}($|\.)")
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching($@"^{RegexEscape(ComponentsNamespace)}($|\.)")
            )
            .Check(Architecture);
    }

    [Test]
    public void Generator_Namespaces_Should_Not_Form_Cycles()
    {
        var creator = new SliceRuleCreator();
        creator.SetSliceAssignment(
            new SliceAssignment(
                type =>
                {
                    var ns = type.Namespace.FullName;
                    if (IsInOrUnder(ns, GeneratorNamespace))
                    {
                        return SliceIdentifier.Of("Generator");
                    }

                    if (IsInOrUnder(ns, ComponentsNamespace))
                    {
                        return SliceIdentifier.Of("Components");
                    }

                    return SliceIdentifier.Ignore();
                },
                "Generator vs Components"
            )
        );

        new GivenSlices(creator).Should().BeFreeOfCycles().Check(Architecture);
    }

    private static bool IsInOrUnder(string ns, string root) =>
        ns == root || ns.StartsWith(root + ".", StringComparison.Ordinal);

    private static string RegexEscape(string value) =>
        System.Text.RegularExpressions.Regex.Escape(value);
}
