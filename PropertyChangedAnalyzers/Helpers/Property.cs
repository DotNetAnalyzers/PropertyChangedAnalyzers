namespace PropertyChangedAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Property
    {
        internal static ExpressionSyntax? FindSingleMutated(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return property.TryGetSetter(out var setter)
                  ? Setter.FindSingleMutated(setter, semanticModel, cancellationToken)
                  : null;
        }

        internal static ExpressionSyntax? FindSingleReturned(PropertyDeclarationSyntax property)
        {
            if (property.ExpressionBody is { Expression: { } expression })
            {
                return expression;
            }

            return property.TryGetGetter(out var getter)
                   ? Getter.FindSingleReturned(getter)
                   : null;
        }

        internal static bool IsLazy(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (propertyDeclaration.TryGetSetter(out _))
            {
                return false;
            }

            if (propertyDeclaration.TryGetGetter(out var getter))
            {
                if (getter is { Body: null, ExpressionBody: null })
                {
                    return false;
                }

                using (var walker = ReturnExpressionsWalker.Borrow(getter))
                {
                    foreach (var returnValue in walker.ReturnValues)
                    {
                        if (IsCoalesceAssign(returnValue))
                        {
                            return true;
                        }

                        if (semanticModel.TryGetSymbol(returnValue, cancellationToken, out IFieldSymbol? returnedField) &&
                            AssignmentExecutionWalker.FirstFor(returnedField, getter, SearchScope.Instance, semanticModel, cancellationToken, out _))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            return propertyDeclaration.ExpressionBody is { } arrow &&
                   IsCoalesceAssign(arrow.Expression);

            bool IsCoalesceAssign(ExpressionSyntax expression)
            {
                return expression is BinaryExpressionSyntax binary &&
                       binary.IsKind(SyntaxKind.CoalesceExpression) &&
                       semanticModel.TryGetSymbol(binary.Left, cancellationToken, out IFieldSymbol? coalesceField) &&
                       AssignmentExecutionWalker.FirstFor(coalesceField, binary.Right, SearchScope.Instance, semanticModel, cancellationToken, out _);
            }
        }

        internal static bool ShouldNotify(PropertyDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (declaration is null ||
                declaration.Modifiers.Any(SyntaxKind.PrivateKeyword) ||
                declaration.Modifiers.Any(SyntaxKind.ProtectedKeyword) ||
                declaration.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                declaration.Modifiers.Any(SyntaxKind.AbstractKeyword) ||
                !declaration.TryGetSetter(out _) ||
                declaration.ExpressionBody != null ||
                IsAutoPropertyMutatedOnlyInConstructor(declaration))
            {
                return false;
            }

            return semanticModel.TryGetSymbol(declaration, cancellationToken, out var property) &&
                   ShouldNotify(declaration, property, semanticModel, cancellationToken);
        }

        internal static bool ShouldNotify(PropertyDeclarationSyntax declaration, IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (property.IsIndexer ||
                property.DeclaredAccessibility == Accessibility.Private ||
                property.DeclaredAccessibility == Accessibility.Protected ||
                property.IsStatic ||
                property.IsReadOnly ||
                property.GetMethod is null ||
                property.IsAbstract ||
                property.ContainingType is null ||
                property.ContainingType.IsValueType ||
                property.ContainingType.DeclaredAccessibility == Accessibility.Private ||
                property.ContainingType.DeclaredAccessibility == Accessibility.Protected ||
                IsAutoPropertyMutatedOnlyInConstructor(declaration))
            {
                return false;
            }

            if (IsMutableAutoProperty(declaration))
            {
                return true;
            }

            if (declaration.TryGetSetter(out var setter))
            {
                if (Setter.AssignsValueToBackingField(setter) is { } assignment)
                {
                    return PropertyChanged.InvokesPropertyChangedFor(assignment, property, semanticModel, cancellationToken) == AnalysisResult.No;
                }

                return false;
            }

            return false;
        }

        internal static bool IsMutableAutoProperty(PropertyDeclarationSyntax property)
        {
            return IsMutableAutoProperty(property, out _, out _);
        }

        internal static bool IsMutableAutoProperty(PropertyDeclarationSyntax property, [NotNullWhen(true)] out AccessorDeclarationSyntax? getter, [NotNullWhen(true)] out AccessorDeclarationSyntax? setter)
        {
            if (property.TryGetGetter(out getter) &&
                getter.Body is null &&
                getter.ExpressionBody is null &&
                property.TryGetSetter(out setter) &&
                setter.Body is null &&
                setter.ExpressionBody is null)
            {
                return true;
            }

            getter = null;
            setter = null;
            return false;
        }

        internal static bool TryGetAssignedProperty(AssignmentExpressionSyntax assignment, [NotNullWhen(true)] out PropertyDeclarationSyntax? propertyDeclaration)
        {
            propertyDeclaration = null;
            switch (assignment)
            {
                case { Left: IdentifierNameSyntax identifierName }:
                    return assignment.TryFirstAncestor(out TypeDeclarationSyntax? containingType) &&
                           containingType.TryFindProperty(identifierName.Identifier.ValueText, out propertyDeclaration);
                case { Left: MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax _, Name: { } name } }:
                    return assignment.TryFirstAncestor(out containingType) &&
                           containingType.TryFindProperty(name.Identifier.ValueText, out propertyDeclaration);
                default:
                    return false;
            }
        }

        internal static bool IsAutoPropertyMutatedOnlyInConstructor(this PropertyDeclarationSyntax property)
        {
            if (property is { ExpressionBody: null, Parent: ClassDeclarationSyntax containingClass } &&
                property.TryGetGetter(out var getter) &&
                getter is { ExpressionBody: null, Body: null })
            {
                if (property.TryGetSetter(out var setter))
                {
                    if (property.Modifiers.Any(SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword) &&
                        !setter.Modifiers.Any(SyntaxKind.PrivateKeyword))
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }

                var name = property.Identifier.ValueText;
                var mutatedInConstructor = false;
                using var walker = IdentifierNameWalker.Borrow(containingClass);
                foreach (var identifierName in walker.IdentifierNames)
                {
                    if (identifierName.Identifier.ValueText == name &&
                        IsMutation(identifierName))
                    {
                        if (IsInConstructor(identifierName))
                        {
                            mutatedInConstructor = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return mutatedInConstructor;
            }

            return false;

            static bool IsMutation(IdentifierNameSyntax identifierName)
            {
                var parent = identifierName.Parent;
                if (parent is MemberAccessExpressionSyntax memberAccess)
                {
                    if (memberAccess.Expression is ThisExpressionSyntax)
                    {
                        parent = memberAccess.Parent;
                    }
                    else
                    {
                        return false;
                    }
                }

                return parent switch
                {
                    AssignmentExpressionSyntax a => a.Left.Contains(identifierName),
                    PostfixUnaryExpressionSyntax _ => true,
                    PrefixUnaryExpressionSyntax p => !p.IsKind(SyntaxKind.LogicalNotExpression),
                    _ => false,
                };
            }

            bool IsInConstructor(SyntaxNode node)
            {
                if (node.TryFirstAncestor(out ConstructorDeclarationSyntax? ctor) &&
                    property.Modifiers.Any(SyntaxKind.StaticKeyword) == ctor.Modifiers.Any(SyntaxKind.StaticKeyword))
                {
                    // Could be in an event handler in ctor.
                    return !node.TryFirstAncestor<AnonymousFunctionExpressionSyntax>(out _);
                }

                return false;
            }
        }
    }
}
