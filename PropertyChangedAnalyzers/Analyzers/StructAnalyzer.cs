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
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is StructDeclarationSyntax { BaseList: { } baseList } declaration &&
                context.ContainingSymbol is INamedTypeSymbol type &&
                type.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation))
            {
                var location = baseList.Types.TryFirst(x => x == KnownSymbol.INotifyPropertyChanged, out var inpc)
                    ? inpc.GetLocation()
                    : declaration.Identifier.GetLocation();
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC008StructMustNotNotify, location, context.ContainingSymbol.Name));
            }
        }
    }
}
