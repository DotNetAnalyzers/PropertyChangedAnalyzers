namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
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
            INPC010SetAndReturnSameField.Descriptor,
            INPC015PropertyIsRecursive.Descriptor);

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
                context.ContainingSymbol is IPropertySymbol property)
            {
                if (propertyDeclaration.ExpressionBody is ArrowExpressionClauseSyntax expressionBody &&
                    IsProperty(expressionBody.Expression, property))
                {
                    context.ReportDiagnostic(Diagnostic.Create(INPC015PropertyIsRecursive.Descriptor, expressionBody.Expression.GetLocation(), "Expression body returns property, infinite recursion"));
                }

                if (propertyDeclaration.TryGetGetAccessorDeclaration(out var getter))
                {
                    using (var returnWalker = ReturnExpressionsWalker.Borrow(getter))
                    {
                        if (returnWalker.ReturnValues.TryFirst(x => IsProperty(x, property), out var recursiveReturnValue))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC015PropertyIsRecursive.Descriptor, recursiveReturnValue.GetLocation(), "Getter returns property, infinite recursion"));
                        }
                    }
                }

                if (propertyDeclaration.TryGetSetAccessorDeclaration(out var setter))
                {
                    using (var assignmentWalker = AssignmentWalker.Borrow(setter))
                    {
                        if (assignmentWalker.Assignments.TryFirst(x => IsProperty(x.Left, property), out var recursiveAssignment))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC015PropertyIsRecursive.Descriptor, recursiveAssignment.Left.GetLocation(), "Setter assigns property, infinite recursion"));
                        }
                    }
                }

                if (Property.TrySingleReturnedInGetter(propertyDeclaration, out var returnValue) &&
                    Property.TryGetBackingFieldFromSetter(propertyDeclaration, context.SemanticModel, context.CancellationToken, out var assigned) &&
                    context.SemanticModel.GetSymbolSafe(returnValue, context.CancellationToken) is ISymbol returned &&
                    !ReferenceEquals(returned, assigned))
                {
                    context.ReportDiagnostic(Diagnostic.Create(INPC010SetAndReturnSameField.Descriptor, propertyDeclaration.GetLocation()));
                }

                if (property.ContainingType.Is(KnownSymbol.INotifyPropertyChanged) &&
                    Property.ShouldNotify(propertyDeclaration, property, context.SemanticModel, context.CancellationToken))
                {
                    context.ReportDiagnostic(Diagnostic.Create(INPC002MutablePublicPropertyShouldNotify.Descriptor, propertyDeclaration.GetLocation(), property.Name));
                }
            }
        }

        private static bool IsProperty(ExpressionSyntax expression, IPropertySymbol property)
        {
            if (property.ExplicitInterfaceImplementations.Any())
            {
                return false;
            }

            if (expression is IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.ValueText == property.Name;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is ThisExpressionSyntax &&
                memberAccess.Name is IdentifierNameSyntax name)
            {
                return name.Identifier.ValueText == property.Name;
            }

            return false;
        }
    }
}
