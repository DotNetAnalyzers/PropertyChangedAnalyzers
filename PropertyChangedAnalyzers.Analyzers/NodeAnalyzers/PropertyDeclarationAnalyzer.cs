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
            INPC010GetAndSetSame.Descriptor,
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

                if (propertyDeclaration.TryGetGetter(out var getter))
                {
                    using (var returnWalker = ReturnExpressionsWalker.Borrow(getter))
                    {
                        if (returnWalker.ReturnValues.TryFirst(x => IsProperty(x, property), out var recursiveReturnValue))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC015PropertyIsRecursive.Descriptor, recursiveReturnValue.GetLocation(), "Getter returns property, infinite recursion"));
                        }
                    }
                }

                if (propertyDeclaration.TryGetSetter(out var setter))
                {
                    using (var assignmentWalker = AssignmentWalker.Borrow(setter))
                    {
                        if (assignmentWalker.Assignments.TryFirst(x => IsProperty(x.Left, property), out var recursiveAssignment))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC015PropertyIsRecursive.Descriptor, recursiveAssignment.Left.GetLocation(), "Setter assigns property, infinite recursion"));
                        }

                        if (getter != null)
                        {
                            if (property.ContainingType.Is(KnownSymbol.INotifyPropertyChanged) &&
                                Property.ShouldNotify(propertyDeclaration, property, context.SemanticModel, context.CancellationToken))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC002MutablePublicPropertyShouldNotify.Descriptor, propertyDeclaration.GetLocation(), property.Name));
                            }

                            if (assignmentWalker.Assignments.TrySingle(out var singleAssignment) &&
                                ReturnExpressionsWalker.TryGetSingle(getter, out var singleReturnValue) &&
                                !MemberPath.Uses(singleAssignment.Left, singleReturnValue, context))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC010GetAndSetSame.Descriptor, propertyDeclaration.GetLocation()));
                            }
                        }
                    }
                }
            }
        }

        private static bool IsProperty(ExpressionSyntax expression, IPropertySymbol property)
        {
            if (expression is MemberAccessExpressionSyntax memberAccess &&
                !(memberAccess.Expression is ThisExpressionSyntax))
            {
                return false;
            }

            if (property.ExplicitInterfaceImplementations.Any())
            {
                return false;
            }

            if (TryGetMemberName(expression, out var name))
            {
                return name == property.Name;
            }

            return false;
        }

        private static bool TryGetMemberName(ExpressionSyntax expression, out string name)
        {
            if (expression is IdentifierNameSyntax identifierName)
            {
                name = identifierName.Identifier.ValueText;
                return true;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is InstanceExpressionSyntax &&
                memberAccess.Name is IdentifierNameSyntax nameIdentifier)
            {
                name = nameIdentifier.Identifier.ValueText;
                return true;
            }

            name = null;
            return false;
        }
    }
}
