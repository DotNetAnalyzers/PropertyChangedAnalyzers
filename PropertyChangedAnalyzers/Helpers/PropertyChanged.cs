namespace PropertyChangedAnalyzers
{
    using System;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyChanged
    {
        internal static AnalysisResult InvokesPropertyChangedFor(ExpressionSyntax mutation, IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (mutation.Parent is ArgumentSyntax argument &&
                argument.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword) &&
                argument.Parent is ArgumentListSyntax argumentList &&
                argumentList.Parent is InvocationExpressionSyntax invocation &&
                invocation.IsPotentialThisOrBase())
            {
                if (TrySet.IsMatch(invocation, semanticModel, cancellationToken) != AnalysisResult.No &&
                    semanticModel.GetSymbolSafe(invocation, cancellationToken) is IMethodSymbol setAndRaiseMethod &&
                    setAndRaiseMethod.Parameters.TryLast(x => x.Type == KnownSymbol.String, out var nameParameter))
                {
                    if (invocation.TryFindArgument(nameParameter, out var nameArg))
                    {
                        if (nameArg.TryGetStringValue(semanticModel, cancellationToken, out var name))
                        {
                            if (string.IsNullOrEmpty(name) ||
                                name == property.Name)
                            {
                                return AnalysisResult.Yes;
                            }
                        }
                    }
                    else if (invocation.FirstAncestor<PropertyDeclarationSyntax>() is PropertyDeclarationSyntax propertyDeclaration &&
                             propertyDeclaration.Identifier.ValueText == property.Name)
                    {
                        return AnalysisResult.Yes;
                    }
                }
                else if (semanticModel.GetSymbolSafe(invocation, cancellationToken) is IMethodSymbol method &&
                         method.TrySingleDeclaration(cancellationToken, out MethodDeclarationSyntax declaration))
                {
                    switch (Notifies(declaration))
                    {
                        case AnalysisResult.No:
                            break;
                        case AnalysisResult.Yes:
                            return AnalysisResult.Yes;
                        case AnalysisResult.Maybe:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else if (mutation is AssignmentExpressionSyntax assignmentExpression &&
                     (assignmentExpression.Left is IdentifierNameSyntax ||
                      (assignmentExpression.Left is MemberAccessExpressionSyntax memberAccess &&
                       memberAccess.Expression is ThisExpressionSyntax)) &&
                      semanticModel.TryGetSymbol(assignmentExpression.Left, cancellationToken, out IPropertySymbol? otherProperty) &&
                      otherProperty.SetMethod.TrySingleDeclaration(cancellationToken, out AccessorDeclarationSyntax? otherSetter) &&
                    Notifies(otherSetter) == AnalysisResult.Yes)
            {
                return AnalysisResult.Yes;
            }

            var block = mutation.FirstAncestorOrSelf<MethodDeclarationSyntax>()?.Body ??
                        mutation.FirstAncestorOrSelf<AccessorDeclarationSyntax>()?.Body ??
                        mutation.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>()?.Body;

            return Notifies(block);

            AnalysisResult Notifies(SyntaxNode scope)
            {
                if (scope == null)
                {
                    return AnalysisResult.No;
                }

                var result = AnalysisResult.No;
                using (var walker = InvocationWalker.Borrow(scope))
                {
                    foreach (var candidate in walker.Invocations)
                    {
                        if (!candidate.Contains(mutation) &&
                            mutation.IsExecutedBefore(candidate) == ExecutedBefore.No)
                        {
                            continue;
                        }

                        switch (TryGetName(candidate, semanticModel, cancellationToken, out var propertyName))
                        {
                            case AnalysisResult.No:
                                continue;
                            case AnalysisResult.Yes:
                                if (string.IsNullOrEmpty(propertyName) ||
                                    propertyName == property.Name)
                                {
                                    return AnalysisResult.Yes;
                                }

                                continue;
                            case AnalysisResult.Maybe:
                                result = AnalysisResult.Maybe;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                return result;
            }
        }

        internal static AnalysisResult TryGetName(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out string propertyName)
        {
            propertyName = null;
            if (PropertyChangedEvent.IsInvoke(invocation, semanticModel, cancellationToken))
            {
                if (invocation.ArgumentList.Arguments.TryElementAt(1, out var propertyChangedArg) &&
                    PropertyChangedEventArgs.TryGetPropertyName(propertyChangedArg.Expression, semanticModel, cancellationToken, out propertyName))
                {
                    return AnalysisResult.Yes;
                }

                return AnalysisResult.No;
            }

            if (OnPropertyChanged.IsMatch(invocation, semanticModel, cancellationToken, out var onPropertyChanged) != AnalysisResult.No &&
                onPropertyChanged.Parameters.TrySingle(out var parameter))
            {
                if (invocation.ArgumentList.Arguments.Count == 0)
                {
                    if (parameter.IsCallerMemberName())
                    {
                        switch (invocation.FirstAncestorOrSelf<MemberDeclarationSyntax>())
                        {
                            case MethodDeclarationSyntax method:
                                propertyName = method.Identifier.ValueText;
                                return AnalysisResult.Yes;
                            case PropertyDeclarationSyntax property:
                                propertyName = property.Identifier.ValueText;
                                return AnalysisResult.Yes;
                        }
                    }

                    return AnalysisResult.No;
                }

                if (invocation.ArgumentList.Arguments.TrySingle(out var argument))
                {
                    if (parameter.Type == KnownSymbol.String)
                    {
                        return argument.TryGetStringValue(semanticModel, cancellationToken, out propertyName)
                            ? AnalysisResult.Yes
                            : AnalysisResult.No;
                    }

                    if (parameter.Type == KnownSymbol.PropertyChangedEventArgs)
                    {
                        return PropertyChangedEventArgs.TryGetPropertyName(argument.Expression, semanticModel, cancellationToken, out propertyName)
                            ? AnalysisResult.Yes
                            : AnalysisResult.No;
                    }

                    if (argument.Expression is ParenthesizedLambdaExpressionSyntax lambda)
                    {
                        if (semanticModel.GetSymbolSafe(lambda.Body, cancellationToken) is ISymbol property)
                        {
                            propertyName = property.Name;
                            return AnalysisResult.Yes;
                        }

                        return AnalysisResult.No;
                    }
                }
            }

            return AnalysisResult.No;
        }
    }
}
