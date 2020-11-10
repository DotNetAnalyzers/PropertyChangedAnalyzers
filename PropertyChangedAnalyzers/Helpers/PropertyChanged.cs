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
            if (mutation.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } } argument &&
                argument.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword) &&
                invocation.IsPotentialThisOrBase())
            {
                if (TrySet.IsMatch(invocation, semanticModel, cancellationToken) != AnalysisResult.No &&
                    semanticModel.GetSymbolSafe(invocation, cancellationToken) is { } setAndRaiseMethod &&
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
                    else if (invocation.FirstAncestor<PropertyDeclarationSyntax>() is { } propertyDeclaration &&
                             propertyDeclaration.Identifier.ValueText == property.Name)
                    {
                        return AnalysisResult.Yes;
                    }
                }
                else if (semanticModel.GetSymbolSafe(invocation, cancellationToken) is { } method &&
                         method.TrySingleDeclaration(cancellationToken, out MethodDeclarationSyntax? declaration))
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
                            throw new InvalidOperationException("Unknown AnalysisResult");
                    }
                }
            }
            else if (mutation is AssignmentExpressionSyntax assignmentExpression &&
                     (assignmentExpression.Left is IdentifierNameSyntax ||
                      assignmentExpression.Left is MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax _ }) &&
                     semanticModel.TryGetSymbol(assignmentExpression.Left, cancellationToken, out IPropertySymbol? otherProperty) &&
                     otherProperty.SetMethod is { } &&
                     otherProperty.SetMethod.TrySingleDeclaration(cancellationToken, out AccessorDeclarationSyntax? otherSetter) &&
                    Notifies(otherSetter) == AnalysisResult.Yes)
            {
                return AnalysisResult.Yes;
            }

            var block = mutation.FirstAncestorOrSelf<MethodDeclarationSyntax>()?.Body ??
                                    mutation.FirstAncestorOrSelf<AccessorDeclarationSyntax>()?.Body ??
                                    mutation.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>()?.Body;

            return Notifies(block);

            AnalysisResult Notifies(SyntaxNode? scope)
            {
                if (scope is null)
                {
                    return AnalysisResult.No;
                }

                var result = AnalysisResult.No;
                using var walker = InvocationWalker.Borrow(scope);
                foreach (var candidate in walker.Invocations)
                {
                    if (!candidate.Contains(mutation) &&
                        mutation.IsExecutedBefore(candidate) == ExecutedBefore.No)
                    {
                        continue;
                    }

                    switch (FindPropertyName(candidate, semanticModel, cancellationToken))
                    {
                        case null:
                            continue;
                        case { Name: var propertyName }:
                            if (string.IsNullOrEmpty(propertyName) ||
                                propertyName == property.Name)
                            {
                                return AnalysisResult.Yes;
                            }

                            continue;
                    }
                }

                return result;
            }
        }

        internal static PropertyNameArgument? FindPropertyName(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            switch (invocation)
            {
                case { ArgumentList: { Arguments: { Count: 0 } } }
                    when OnPropertyChanged.Match(invocation, semanticModel, cancellationToken) is { Name: { } parameter } &&
                         parameter.IsCallerMemberName():
                    return invocation.FirstAncestorOrSelf<MemberDeclarationSyntax>() switch
                    {
                        MethodDeclarationSyntax declaration => new PropertyNameArgument(null, declaration.Identifier.ValueText),
                        PropertyDeclarationSyntax declaration => new PropertyNameArgument(null, declaration.Identifier.ValueText),
                        EventDeclarationSyntax declaration => new PropertyNameArgument(null, declaration.Identifier.ValueText),
                        IndexerDeclarationSyntax _ => new PropertyNameArgument(null, "this[]"),
                        _ => null,
                    };
                case { ArgumentList: { Arguments: { Count: 1 } arguments } }
                    when arguments[0] is { } argument &&
                         OnPropertyChanged.Match(invocation, semanticModel, cancellationToken) is { Name: { } parameter }:
                    if (parameter.Type.SpecialType == SpecialType.System_String)
                    {
                        return PropertyNameArgument.Match(argument, semanticModel, cancellationToken);
                    }

                    if (PropertyChangedEventArgs.Match(argument.Expression, semanticModel, cancellationToken) is { } eventArgs)
                    {
                        return eventArgs.PropertyName(semanticModel, cancellationToken);
                    }

                    if (argument is { Expression: ParenthesizedLambdaExpressionSyntax { Body: { } body } } &&
                        semanticModel.GetSymbolSafe(body, cancellationToken) is { } property)
                    {
                        return new PropertyNameArgument(argument, property.Name);
                    }

                    break;

                case { ArgumentList: { Arguments: { Count: 2 } } }
                    when Invoke.Match(invocation, semanticModel, cancellationToken) is { EventArgument: { } arg } &&
                         PropertyChangedEventArgs.Match(arg.Expression, semanticModel, cancellationToken) is { } propertyChangedEventArgs:
                    return propertyChangedEventArgs.PropertyName(semanticModel, cancellationToken);
                default:
                    return null;
            }

            return null;
        }

        internal static IEventSymbol? Find(ITypeSymbol type)
        {
            return type.TryFindEventRecursive("PropertyChanged", out var propertyChangedEvent)
                ? propertyChangedEvent
                : null;
        }

        internal readonly struct Invoke
        {
            internal readonly InvocationExpressionSyntax Invocation;

            private Invoke(InvocationExpressionSyntax invocation)
            {
                this.Invocation = invocation;
            }

            internal ArgumentSyntax EventArgument => this.Invocation.ArgumentList.Arguments[1];

            internal static Invoke? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (invocation is { ArgumentList: { Arguments: { Count: 2 } arguments } } &&
                    arguments[0].Expression.IsEither(SyntaxKind.ThisExpression, SyntaxKind.NullLiteralExpression) &&
                    invocation.IsPotentialReturnVoid())
                {
                    return invocation.Parent switch
                    {
                        ConditionalAccessExpressionSyntax { Expression: IdentifierNameSyntax _ }
                            when semanticModel.TryGetSymbol(invocation, KnownSymbol.PropertyChangedEventHandler.Invoke, cancellationToken, out _)
                            => new Invoke(invocation),
                        ConditionalAccessExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax _, Name: IdentifierNameSyntax _ } }
                            when semanticModel.TryGetSymbol(invocation, KnownSymbol.PropertyChangedEventHandler.Invoke, cancellationToken, out _)
                            => new Invoke(invocation),
                        ExpressionStatementSyntax _
                            when semanticModel.TryGetSymbol(invocation, cancellationToken, out var symbol) &&
                                 symbol == KnownSymbol.PropertyChangedEventHandler.Invoke
                            => new Invoke(invocation),
                        _ => null,
                    };
                }

                return null;
            }
        }
    }
}
