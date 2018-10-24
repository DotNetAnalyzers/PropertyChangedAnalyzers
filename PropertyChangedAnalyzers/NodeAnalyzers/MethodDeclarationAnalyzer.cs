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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(INPC004UseCallerMemberName.Descriptor);

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
                context.Node is MethodDeclarationSyntax methodDeclaration &&
                methodDeclaration.ParameterList is ParameterListSyntax parameterList &&
                parameterList.Parameters.TrySingle(out var parameterSyntax) &&
                method.Parameters.TrySingle(out var parameter) &&
                parameter.Type == KnownSymbol.String &&
                !parameter.IsCallerMemberName() &&
                PropertyChanged.IsOnPropertyChanged(method, context.SemanticModel, context.CancellationToken) == AnalysisResult.Yes)
            {
                context.ReportDiagnostic(Diagnostic.Create(INPC004UseCallerMemberName.Descriptor, parameterSyntax.GetLocation()));
            }
        }
    }
}
