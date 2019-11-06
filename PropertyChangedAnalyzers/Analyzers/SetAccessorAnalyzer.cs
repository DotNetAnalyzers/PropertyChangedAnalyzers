namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Runtime.InteropServices.ComTypes;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class SetAccessorAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC002MutablePublicPropertyShouldNotify);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.SetAccessorDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is AccessorDeclarationSyntax { Parent: AccessorListSyntax { Parent: PropertyDeclarationSyntax containingProperty } } setter &&
                context.ContainingSymbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol property } &&
                property.ContainingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation))
            {
                switch (setter)
                {
                    case { ExpressionBody: { Expression: { } expression } }:
                        if (expression.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                            Property.ShouldNotify(containingProperty, property, context.SemanticModel, context.CancellationToken))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.INPC002MutablePublicPropertyShouldNotify,
                                    containingProperty.Identifier.GetLocation(),
                                    property.Name));
                        }

                        break;
                    case { Body: { } }:
                        if (Property.ShouldNotify(containingProperty, property, context.SemanticModel, context.CancellationToken))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.INPC002MutablePublicPropertyShouldNotify,
                                    containingProperty.Identifier.GetLocation(),
                                    property.Name));
                        }

                        break;
                }
            }
        }
    }
}
