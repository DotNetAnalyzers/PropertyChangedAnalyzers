namespace PropertyChangedAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Property
    {
        internal static bool TryGetContainingProperty(ISymbol symbol, out IPropertySymbol property)
        {
            if (symbol is IMethodSymbol method &&
                method.AssociatedSymbol is ISymbol associated)
            {
                property = associated as IPropertySymbol;
            }
            else
            {
                property = symbol.ContainingSymbol as IPropertySymbol;
            }

            return property != null;
        }

        internal static bool IsLazy(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (propertyDeclaration.TryGetSetter(out _))
            {
                return false;
            }

            IFieldSymbol returnedField = null;
            if (propertyDeclaration.TryGetGetter(out var getter))
            {
                if (getter.Body == null)
                {
                    return false;
                }

                using (var walker = ReturnExpressionsWalker.Borrow(getter.Body))
                {
                    if (walker.ReturnValues.Count == 0)
                    {
                        return false;
                    }

                    foreach (var returnValue in walker.ReturnValues)
                    {
                        var returnedSymbol = returnValue?.IsKind(SyntaxKind.CoalesceExpression) == true
                            ? semanticModel.GetSymbolSafe((returnValue as BinaryExpressionSyntax)?.Left, cancellationToken) as IFieldSymbol
                            : semanticModel.GetSymbolSafe(returnValue, cancellationToken) as IFieldSymbol;
                        if (returnedSymbol == null)
                        {
                            return false;
                        }

                        if (returnedField != null &&
                            !ReferenceEquals(returnedSymbol, returnedField))
                        {
                            return false;
                        }

                        returnedField = returnedSymbol;
                    }
                }

                return AssignmentWalker.Assigns(returnedField, getter.Body, semanticModel, cancellationToken);
            }

            var arrow = propertyDeclaration.ExpressionBody;
            if (arrow?.Expression?.IsKind(SyntaxKind.CoalesceExpression) != true)
            {
                return false;
            }

            returnedField = semanticModel.GetSymbolSafe((arrow.Expression as BinaryExpressionSyntax)?.Left, cancellationToken) as IFieldSymbol;
            return AssignmentWalker.Assigns(returnedField, arrow.Expression, semanticModel, cancellationToken);
        }

        internal static bool ShouldNotify(PropertyDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (declaration == null ||
                declaration.Modifiers.Any(SyntaxKind.PrivateKeyword) ||
                declaration.Modifiers.Any(SyntaxKind.ProtectedKeyword) ||
                declaration.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                declaration.Modifiers.Any(SyntaxKind.AbstractKeyword) ||
                !declaration.TryGetSetter(out _) ||
                IsAutoPropertyOnlyAssignedInCtor(declaration))
            {
                return false;
            }

            return ShouldNotify(
                declaration,
                semanticModel.GetDeclaredSymbolSafe(declaration, cancellationToken),
                semanticModel,
                cancellationToken);
        }

        internal static bool ShouldNotify(PropertyDeclarationSyntax declaration, IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (property.IsIndexer ||
                property.DeclaredAccessibility == Accessibility.Private ||
                property.DeclaredAccessibility == Accessibility.Protected ||
                property.IsStatic ||
                property.IsReadOnly ||
                property.GetMethod == null ||
                property.IsAbstract ||
                property.ContainingType == null ||
                property.ContainingType.IsValueType ||
                property.ContainingType.DeclaredAccessibility == Accessibility.Private ||
                property.ContainingType.DeclaredAccessibility == Accessibility.Protected ||
                IsAutoPropertyOnlyAssignedInCtor(declaration))
            {
                return false;
            }

            if (IsMutableAutoProperty(declaration))
            {
                return true;
            }

            if (declaration.TryGetSetter(out var setter))
            {
                if (!AssignsValueToBackingField(setter, out var assignment))
                {
                    return false;
                }

                if (PropertyChanged.InvokesPropertyChangedFor(assignment, property, semanticModel, cancellationToken) != AnalysisResult.No)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        internal static bool IsMutableAutoProperty(PropertyDeclarationSyntax property)
        {
            return IsMutableAutoProperty(property, out _, out _);
        }

        internal static bool IsMutableAutoProperty(PropertyDeclarationSyntax property, out AccessorDeclarationSyntax getter, out AccessorDeclarationSyntax setter)
        {
            if (property.TryGetGetter(out getter) &&
                getter.Body == null &&
                getter.ExpressionBody == null &&
                property.TryGetSetter(out setter) &&
                setter.Body == null &&
                setter.ExpressionBody == null)
            {
                return true;
            }

            getter = null;
            setter = null;
            return false;
        }

        internal static bool TrySingleReturnedInGetter(PropertyDeclarationSyntax property, out ExpressionSyntax result)
        {
            result = null;
            if (property == null)
            {
                return false;
            }

            var expressionBody = property.ExpressionBody;
            if (expressionBody != null)
            {
                result = expressionBody.Expression;
                return result != null;
            }

            if (property.TryGetGetter(out var getter))
            {
                expressionBody = getter.ExpressionBody;
                if (expressionBody != null)
                {
                    result = expressionBody.Expression;
                    return result != null;
                }

                var body = getter.Body;
                if (body == null ||
                    body.Statements.Count == 0)
                {
                    return false;
                }

                if (body.Statements.TrySingle(out var statement) &&
                    statement is ReturnStatementSyntax returnStatement)
                {
                    result = returnStatement.Expression;
                    return result != null;
                }
            }

            return false;
        }

        internal static bool TryGetBackingFieldFromSetter(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol field)
        {
            field = null;
            if (property == null)
            {
                return false;
            }

            foreach (var declaration in property.Declarations(cancellationToken))
            {
                var propertyDeclaration = declaration as PropertyDeclarationSyntax;
                if (propertyDeclaration == null)
                {
                    continue;
                }

                return TryGetBackingFieldFromSetter(propertyDeclaration, semanticModel, cancellationToken, out field);
            }

            return false;
        }

        internal static bool TryGetBackingFieldFromSetter(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol field)
        {
            field = null;
            if (property == null)
            {
                return false;
            }

            if (property.TryGetSetter(out var setter))
            {
                if (TryFindSingleSetAndRaise(setter, semanticModel, cancellationToken, out var invocation))
                {
                    return TryGetBackingField(invocation.ArgumentList.Arguments[0].Expression, semanticModel, cancellationToken, out field);
                }

                if (TrySingleAssignmentInSetter(setter, out var assignment))
                {
                    return TryGetBackingField(assignment.Left, semanticModel, cancellationToken, out field);
                }
            }

            return false;
        }

        internal static bool TryFindSingleSetAndRaise(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax invocation)
        {
            invocation = null;
            if (setter == null)
            {
                return false;
            }

            using (var walker = InvocationWalker.Borrow(setter))
            {
                return walker.Invocations.TrySingle(
                    x => PropertyChanged.IsSetAndRaiseCall(
                        x, semanticModel, cancellationToken),
                    out invocation);
            }
        }

        internal static bool TrySingleAssignmentInSetter(AccessorDeclarationSyntax setter, out AssignmentExpressionSyntax assignment)
        {
            assignment = null;
            if (setter == null)
            {
                return false;
            }

            using (var walker = AssignmentWalker.Borrow(setter))
            {
                if (walker.Assignments.TrySingle(out assignment) &&
                    assignment.Right is IdentifierNameSyntax identifierName &&
                    identifierName.Identifier.ValueText == "value")
                {
                    return true;
                }
            }

            assignment = null;
            return false;
        }

        internal static bool AssignsValueToBackingField(AccessorDeclarationSyntax setter, out AssignmentExpressionSyntax assignment)
        {
            using (var walker = AssignmentWalker.Borrow(setter))
            {
                foreach (var a in walker.Assignments)
                {
                    if ((a.Right as IdentifierNameSyntax)?.Identifier.ValueText != "value")
                    {
                        continue;
                    }

                    if (a.Left is IdentifierNameSyntax)
                    {
                        assignment = a;
                        return true;
                    }

                    if (a.Left is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name is IdentifierNameSyntax)
                    {
                        if (memberAccess.Expression is ThisExpressionSyntax ||
                            memberAccess.Expression is IdentifierNameSyntax)
                        {
                            assignment = a;
                            return true;
                        }

                        if (memberAccess.Expression is MemberAccessExpressionSyntax nested &&
                            nested.Expression is ThisExpressionSyntax &&
                            nested.Name is IdentifierNameSyntax)
                        {
                            assignment = a;
                            return true;
                        }
                    }
                }
            }

            assignment = null;
            return false;
        }

        internal static bool TryFindValue(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken, out IParameterSymbol value)
        {
            using (var walker = IdentifierNameWalker.Borrow(setter))
            {
                foreach (var identifierName in walker.IdentifierNames)
                {
                    if (identifierName.Identifier.ValueText == "value")
                    {
                        value = semanticModel.GetSymbolSafe(identifierName, cancellationToken) as IParameterSymbol;
                        if (value != null)
                        {
                            return true;
                        }
                    }
                }
            }

            value = null;
            return false;
        }

        internal static bool TryGetAssignedProperty(AssignmentExpressionSyntax assignment, out PropertyDeclarationSyntax propertyDeclaration)
        {
            propertyDeclaration = null;
            var typeDeclaration = assignment?.FirstAncestor<TypeDeclarationSyntax>();
            if (typeDeclaration == null)
            {
                return false;
            }

            if (assignment.Left is IdentifierNameSyntax identifierName)
            {
                return typeDeclaration.TryFindProperty(identifierName.Identifier.ValueText, out propertyDeclaration);
            }

            if (assignment.Left is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is ThisExpressionSyntax)
            {
                return typeDeclaration.TryFindProperty(memberAccess.Name.Identifier.ValueText, out propertyDeclaration);
            }

            return false;
        }

        private static bool TryGetBackingField(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol field)
        {
            field = null;
            if (candidate is IdentifierNameSyntax)
            {
                field = semanticModel.GetSymbolSafe(candidate, cancellationToken) as IFieldSymbol;
            }
            else if (candidate is MemberAccessExpressionSyntax memberAccess &&
                     memberAccess.Expression is ThisExpressionSyntax)
            {
                field = semanticModel.GetSymbolSafe(candidate, cancellationToken) as IFieldSymbol;
            }

            return field != null;
        }

        private static bool IsAutoPropertyOnlyAssignedInCtor(PropertyDeclarationSyntax propertyDeclaration)
        {
            bool IsAssigned(IdentifierNameSyntax identifierName)
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

                switch (parent)
                {
                    case AssignmentExpressionSyntax a:
                        return a.Left.Contains(identifierName);
                    case PostfixUnaryExpressionSyntax _:
                        return true;
                    case PrefixUnaryExpressionSyntax p:
                        return !p.IsKind(SyntaxKind.LogicalNotExpression);
                    default:
                        return false;
                }
            }

            bool IsInConstructor(SyntaxNode node)
            {
                if (node.FirstAncestor<ConstructorDeclarationSyntax>() == null)
                {
                    return false;
                }

                // Could be in an event handler in ctor.
                return node.FirstAncestor<AnonymousFunctionExpressionSyntax>() == null;
            }

            if (propertyDeclaration.TryGetSetter(out var setter) &&
                setter.Body == null &&
                setter.ExpressionBody == null)
            {
                if (!propertyDeclaration.Modifiers.Any(SyntaxKind.PrivateKeyword) &&
                    !setter.Modifiers.Any(SyntaxKind.PrivateKeyword))
                {
                    return false;
                }

                var name = propertyDeclaration.Identifier.ValueText;
                using (var walker = IdentifierNameWalker.Borrow(propertyDeclaration.FirstAncestor<TypeDeclarationSyntax>()))
                {
                    var isAssigned = false;
                    foreach (var identifierName in walker.IdentifierNames)
                    {
                        if (identifierName.Identifier.ValueText == name &&
                            IsAssigned(identifierName))
                        {
                            isAssigned = true;
                            if (!IsInConstructor(identifierName))
                            {
                                return false;
                            }
                        }
                    }

                    return isAssigned;
                }
            }

            return false;
        }
    }
}
