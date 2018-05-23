namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
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
            INPC015PropertyIsRecursive.Descriptor,
            INPC017BackingFieldNameMustMatch.Descriptor);

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
                if (propertyDeclaration.ExpressionBody is ArrowExpressionClauseSyntax expressionBody)
                {
                    if (IsProperty(expressionBody.Expression, property))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC015PropertyIsRecursive.Descriptor, expressionBody.Expression.GetLocation(), "Expression body returns property, infinite recursion"));
                    }

                    if (expressionBody.Expression is ExpressionSyntax expression &&
                        expression.IsEither(SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.IdentifierName) &&
                        MemberPath.TrySingle(expression, out var single) &&
                        context.SemanticModel.TryGetSymbol(single, context.CancellationToken, out IFieldSymbol backingField) &&
                        !HasMatchingName(backingField, property))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC017BackingFieldNameMustMatch.Descriptor, expressionBody.Expression.GetLocation()));
                    }
                }

                if (propertyDeclaration.TryGetGetter(out var getter))
                {
                    using (var walker = ReturnExpressionsWalker.Borrow(getter))
                    {
                        foreach (var returnValue in walker.ReturnValues)
                        {
                            if (IsProperty(returnValue, property))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC015PropertyIsRecursive.Descriptor, returnValue.GetLocation(), "Getter returns property, infinite recursion"));
                            }

                            if (returnValue != null &&
                                returnValue.IsEither(SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.IdentifierName) &&
                                MemberPath.TrySingle(returnValue, out var single) &&
                                context.SemanticModel.TryGetSymbol(single, context.CancellationToken, out IFieldSymbol backingField) &&
                                !HasMatchingName(backingField, property))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC017BackingFieldNameMustMatch.Descriptor, returnValue.GetLocation()));
                            }
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
                            if (property.ContainingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation) &&
                                Property.ShouldNotify(propertyDeclaration, property, context.SemanticModel, context.CancellationToken))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC002MutablePublicPropertyShouldNotify.Descriptor, propertyDeclaration.GetLocation(), property.Name));
                            }

                            if (assignmentWalker.Assignments.TrySingle(out var singleAssignment) &&
                                ReturnExpressionsWalker.TryGetSingle(getter, out var singleReturnValue) &&
                                !PropertyPath.Uses(singleAssignment.Left, singleReturnValue, context))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC010GetAndSetSame.Descriptor, propertyDeclaration.GetLocation()));
                            }
                        }
                    }
                }
            }
        }

        private static bool HasMatchingName(IFieldSymbol backingField, IPropertySymbol property)
        {
            if (property.ExplicitInterfaceImplementations.TryFirst(out var explicitProperty))
            {
                return HasMatchingName(backingField, explicitProperty);
            }

            if (backingField.Name.Length < property.Name.Length)
            {
                return false;
            }

            var diff = backingField.Name.Length - property.Name.Length;
            for (var pi = property.Name.Length - 1; pi >= 0; pi--)
            {
                var fi = pi + diff;
                if (pi == 0)
                {
                    if (char.ToLower(property.Name[pi]) != backingField.Name[fi])
                    {
                        return false;
                    }

                    switch (fi)
                    {
                        case 0:
                            return true;
                        case 1:
                            return backingField.Name[0] == '_' ||
                                   backingField.Name[0] == '@';
                        default:
                            return false;
                    }
                }

                if (property.Name[pi] != backingField.Name[fi])
                {
                    return false;
                }
            }

            return true;
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
