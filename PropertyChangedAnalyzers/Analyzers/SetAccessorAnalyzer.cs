namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
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
                context.Node is AccessorDeclarationSyntax setter &&
                setter.TryFirstAncestor(out PropertyDeclarationSyntax propertyDeclaration) &&
                context.ContainingSymbol is IMethodSymbol setMethod &&
                setMethod.AssociatedSymbol is IPropertySymbol property &&
                property.ContainingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation))
            {
                if (setter.ExpressionBody is ArrowExpressionClauseSyntax expressionBody)
                {
                    if (expressionBody.Expression.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                        Property.ShouldNotify(propertyDeclaration, property, context.SemanticModel, context.CancellationToken))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC002MutablePublicPropertyShouldNotify, propertyDeclaration.Identifier.GetLocation()));
                    }
                }
                else if (setter.Body != null)
                {
                    if (Property.ShouldNotify(propertyDeclaration, property, context.SemanticModel, context.CancellationToken))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.INPC002MutablePublicPropertyShouldNotify,
                                propertyDeclaration.Identifier.GetLocation(),
                                property.Name));
                    }
                }
                else
                {
                    if (Property.ShouldNotify(propertyDeclaration, property, context.SemanticModel, context.CancellationToken))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.INPC002MutablePublicPropertyShouldNotify,
                                propertyDeclaration.Identifier.GetLocation(),
                                property.Name));
                    }
                }
            }
        }
    }
}
