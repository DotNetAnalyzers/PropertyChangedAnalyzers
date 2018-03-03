namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class PropertyDeclarationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            INPC002MutablePublicPropertyShouldNotify.Descriptor,
            INPC010SetAndReturnSameField.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.PropertyDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is PropertyDeclarationSyntax propertyDeclaration &&
                context.ContainingSymbol is IPropertySymbol propertySymbol)
            {
                if (Property.TrySingleReturnedInGetter(propertyDeclaration, out var returnValue) &&
                    Property.TryGetBackingFieldFromSetter(propertyDeclaration, context.SemanticModel, context.CancellationToken, out var assigned) &&
                    context.SemanticModel.GetSymbolSafe(returnValue, context.CancellationToken) is ISymbol returned &&
                    !ReferenceEquals(returned, assigned))
                {
                    context.ReportDiagnostic(Diagnostic.Create(INPC010SetAndReturnSameField.Descriptor, propertyDeclaration.GetLocation()));
                }

                if (propertySymbol.ContainingType.Is(KnownSymbol.INotifyPropertyChanged) &&
                    Property.ShouldNotify(propertyDeclaration, propertySymbol, context.SemanticModel, context.CancellationToken))
                {
                    context.ReportDiagnostic(Diagnostic.Create(INPC002MutablePublicPropertyShouldNotify.Descriptor, propertyDeclaration.GetLocation(), propertySymbol.Name));
                }
            }
        }
    }
}
