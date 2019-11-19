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
                    if (ShouldBeCallerMemberName(parameter, out var parameterSyntax))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC004UseCallerMemberName, parameterSyntax!.GetLocation()));
                    }

                    if (ShouldBeProtected() is { } location)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.INPC018InvokerShouldBeProtected,
                                location));
                    }
                }
                else if (TrySet.IsMatch(method, context.SemanticModel, context.CancellationToken, out _, out _, out parameter) == AnalysisResult.Yes)
                {
                    if (ShouldBeCallerMemberName(parameter, out var parameterSyntax))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC004UseCallerMemberName, parameterSyntax!.GetLocation()));
                    }

                    if (ShouldBeProtected() is { } location)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.INPC018InvokerShouldBeProtected,
                                location));
                    }
                }
            }

            bool ShouldBeCallerMemberName(IParameterSymbol candidate, out ParameterSyntax? parameterSyntax)
            {
                parameterSyntax = null;
                return !candidate.IsCallerMemberName() &&
                       candidate.Type == KnownSymbol.String &&
                       methodDeclaration.TryFindParameter(candidate.Name, out parameterSyntax) &&
                       CallerMemberNameAttribute.IsAvailable(context.SemanticModel);
            }

            Location? ShouldBeProtected()
            {
                if (method is { DeclaredAccessibility: Accessibility.Private, ContainingType: { IsSealed: false, IsStatic: false } })
                {
                    return methodDeclaration.Modifiers.TryFirst(x => x.IsKind(SyntaxKind.PrivateKeyword), out var modifier)
                        ? modifier.GetLocation()
                        : methodDeclaration.Identifier.GetLocation();
                }

                return null;
            }
        }
    }
}
