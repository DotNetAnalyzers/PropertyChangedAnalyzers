namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class MethodDeclarationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC004UseCallerMemberName,
            Descriptors.INPC018InvokerShouldBeProtected);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.MethodDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.ContainingSymbol is IMethodSymbol method &&
                method.Parameters.TrySingle(out var parameter) &&
                context.Node is MethodDeclarationSyntax methodDeclaration &&
                OnPropertyChanged.IsMatch(method, context.SemanticModel, context.CancellationToken) == AnalysisResult.Yes)
            {
                if (parameter.Type == KnownSymbol.String &&
                    !parameter.IsCallerMemberName() &&
                    methodDeclaration.ParameterList is ParameterListSyntax parameterList &&
                    parameterList.Parameters.TrySingle(out var parameterSyntax))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC004UseCallerMemberName, parameterSyntax.GetLocation()));
                }

                if (method.DeclaredAccessibility == Accessibility.Private &&
                    !method.ContainingType.IsSealed)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.INPC018InvokerShouldBeProtected,
                            methodDeclaration.Modifiers.TryFirst(x => x.IsKind(SyntaxKind.PrivateKeyword), out var modifier) ? modifier.GetLocation() : methodDeclaration.Identifier.GetLocation()));
                }
            }
        }
    }
}
