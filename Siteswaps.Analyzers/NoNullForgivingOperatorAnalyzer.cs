using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Siteswaps.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoNullForgivingOperatorAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "SITESWAPS001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Do not use the null-forgiving operator",
        "The null-forgiving operator is forbidden; handle nullability explicitly",
        "Reliability",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeSuppression,
            SyntaxKind.SuppressNullableWarningExpression);
    }

    private static void AnalyzeSuppression(SyntaxNodeAnalysisContext context)
    {
        if (
            context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(
                "build_property.IsTestProject",
                out var isTestProject
            )
            && bool.TryParse(isTestProject, out var isTest)
            && isTest
        )
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
    }
}
