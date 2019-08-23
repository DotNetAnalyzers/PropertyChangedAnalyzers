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
                context.Node is MethodDeclarationSyntax methodDeclaration)
            {
                if (OnPropertyChanged.IsMatch(method, context.SemanticModel, context.CancellationToken, out var parameter) == AnalysisResult.Yes)
                {
                    if (parameter.Type == KnownSymbol.String &&
                        !parameter.IsCallerMemberName() &&
                        methodDeclaration.ParameterList is ParameterListSyntax parameterList &&
                        parameterList.Parameters.TrySingle(out var parameterSyntax))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC004UseCallerMemberName, parameterSyntax.GetLocation()));
                    }

                    if (ShouldBeProtected(out var location))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.INPC018InvokerShouldBeProtected,
                                location));
                    }
                }
            }

            bool ShouldBeProtected(out Location result)
            {
                if (method.DeclaredAccessibility == Accessibility.Private &&
                    !method.ContainingType.IsSealed)
                {
                    if (methodDeclaration.Modifiers.TryFirst(x => x.IsKind(SyntaxKind.PrivateKeyword), out var modifier))
                    {
                        result = modifier.GetLocation();
                        return true;
                    }

                    result = methodDeclaration.Identifier.GetLocation();
                    return true;
                }

                result = null;
                return false;
            }
        }
    }
}
