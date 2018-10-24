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
            INPC017BackingFieldNameMustMatch.Descriptor,
            INPC019GetBackingField.Descriptor,
            INPC020PreferExpressionBodyAccessor.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.PropertyDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is PropertyDeclarationSyntax propertyDeclaration &&
                context.ContainingSymbol is IPropertySymbol property)
            {
                using (var walker = ReturnExpressions(propertyDeclaration))
                {
                    foreach (var returnValue in walker.ReturnValues)
                    {
                        if (IsProperty(returnValue, property))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC015PropertyIsRecursive.Descriptor, returnValue.GetLocation(), "Getter returns property, infinite recursion"));
                        }
                    }

                    if (walker.ReturnValues.TrySingle(out var single))
                    {
                        if (single.IsEither(SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.IdentifierName) &&
                            MemberPath.TrySingle(single, out var path) &&
                            context.SemanticModel.TryGetSymbol(path, context.CancellationToken, out IFieldSymbol backingField) &&
                            !HasMatchingName(backingField, property))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC017BackingFieldNameMustMatch.Descriptor, path.GetLocation()));
                        }

                        if (single is LiteralExpressionSyntax &&
                            propertyDeclaration.TryGetGetter(out var getter) &&
                            Property.TryGetBackingFieldFromSetter(propertyDeclaration, context.SemanticModel, context.CancellationToken, out var field))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC019GetBackingField.Descriptor, single.GetLocation()));
                        }
                    }
                }

                if (propertyDeclaration.TryGetSetter(out var setter))
                {
                    if (ShouldBeExpressionBody(setter))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC020PreferExpressionBodyAccessor.Descriptor, setter.GetLocation()));
                    }

                    using (var assignmentWalker = AssignmentWalker.Borrow(setter))
                    {
                        if (assignmentWalker.Assignments.TryFirst(x => IsProperty(x.Left, property), out var recursiveAssignment))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC015PropertyIsRecursive.Descriptor, recursiveAssignment.Left.GetLocation(), "Setter assigns property, infinite recursion"));
                        }

                        if (propertyDeclaration.TryGetGetter(out var getter))
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

                            if (ShouldBeExpressionBody(getter))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC020PreferExpressionBodyAccessor.Descriptor, getter.GetLocation()));
                            }
                        }
                    }
                }
            }
        }

        private static ReturnExpressionsWalker ReturnExpressions(PropertyDeclarationSyntax propertyDeclaration)
        {
            if (propertyDeclaration.TryGetGetter(out var getter))
            {
                return ReturnExpressionsWalker.Borrow(getter);
            }

            if (propertyDeclaration.ExpressionBody is ArrowExpressionClauseSyntax expressionBody)
            {
                return ReturnExpressionsWalker.Borrow(expressionBody);
            }

            return ReturnExpressionsWalker.Empty();
        }

        private static bool HasMatchingName(IFieldSymbol backingField, IPropertySymbol property)
        {
            if (backingField.IsStatic || backingField.IsConst)
            {
                return true;
            }

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
                    if (pi == 0 ||
                        !char.IsUpper(property.Name[pi - 1]) ||
                        char.ToUpper(backingField.Name[fi]) != property.Name[pi])
                    {
                        return false;
                    }
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

        private static bool ShouldBeExpressionBody(AccessorDeclarationSyntax accessor)
        {
            return accessor.Body is BlockSyntax block &&
                   block.Statements.TrySingle(out var statement) &&
                   statement.IsEither(SyntaxKind.ReturnStatement, SyntaxKind.ExpressionStatement);
        }
    }
}
