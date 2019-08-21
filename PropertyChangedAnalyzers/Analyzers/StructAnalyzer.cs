namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class StructAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.INPC008StructMustNotNotify);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.StructDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is StructDeclarationSyntax structDeclaration &&
                context.ContainingSymbol is INamedTypeSymbol type &&
                type.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC008StructMustNotNotify, GetNode().GetLocation(), context.ContainingSymbol.Name));
            }

            SyntaxNode GetNode()
            {
                if (structDeclaration.BaseList != null &&
                    structDeclaration.BaseList.Types.TryFirst(x => x == KnownSymbol.INotifyPropertyChanged, out var inpc))
                {
                    return inpc;
                }

                return structDeclaration;
            }
        }
    }
}
