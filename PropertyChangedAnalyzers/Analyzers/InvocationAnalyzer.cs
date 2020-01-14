namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class InvocationAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC009DoNotRaiseChangeForMissingProperty);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax invocation &&
                invocation.ArgumentList is { Arguments: { Count: 0 } } &&
                !(invocation.FirstAncestor<AccessorDeclarationSyntax>() is { Keyword: { ValueText: "set" } }) &&
                PropertyChanged.FindPropertyName(invocation, context.SemanticModel, context.CancellationToken) is { })
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009DoNotRaiseChangeForMissingProperty, invocation.GetLocation()));
            }
        }
    }
}
