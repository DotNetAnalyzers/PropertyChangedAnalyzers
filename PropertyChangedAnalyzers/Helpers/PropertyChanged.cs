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
                        case { Result: AnalysisResult.No }:
                            continue;
                        case { Result: AnalysisResult.Yes, Value: var propertyName }:
                            if (string.IsNullOrEmpty(propertyName) ||
                                propertyName == property.Name)
                            {
                                return AnalysisResult.Yes;
                            }

                            continue;
                        case { Result: AnalysisResult.Maybe }:
                            result = AnalysisResult.Maybe;
                            break;
                        default:
                            throw new InvalidOperationException("Unknown AnalysisResult");
                    }
                }

                return result;
            }
        }

        internal static AnalysisResult<string?>? FindPropertyName(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (Invoke.Match(invocation, semanticModel, cancellationToken) is { EventArgument: { } arg })
            {
                return PropertyChangedEventArgs.FindPropertyName(arg.Expression, semanticModel, cancellationToken);
            }

            if (OnPropertyChanged.Match(invocation, semanticModel, cancellationToken) is { Name: { } parameter })
            {
                switch (invocation)
                {
                    case { ArgumentList: { Arguments: { Count: 0 } } }:
                        if (parameter.IsCallerMemberName())
                        {
                            return invocation.FirstAncestorOrSelf<MemberDeclarationSyntax>() switch
                            {
                                MethodDeclarationSyntax declaration => new AnalysisResult<string?>(AnalysisResult.Yes, declaration.Identifier.ValueText),
                                PropertyDeclarationSyntax declaration => new AnalysisResult<string?>(AnalysisResult.Yes, declaration.Identifier.ValueText),
                                EventDeclarationSyntax declaration => new AnalysisResult<string?>(AnalysisResult.Yes, declaration.Identifier.ValueText),
                                IndexerDeclarationSyntax _ => new AnalysisResult<string?>(AnalysisResult.Yes, "this[]"),
                                _ => default,
                            };
                        }

                        return new AnalysisResult<string?>(AnalysisResult.Maybe, parameter.ExplicitDefaultValue as string);
                    case { ArgumentList: { Arguments: { Count: 1 } arguments } }
                        when arguments[0] is { } argument &&
                             parameter.Type.SpecialType == SpecialType.System_String:
                        return argument.TryGetStringValue(semanticModel, cancellationToken, out var propertyName)
                            ? new AnalysisResult<string?>(AnalysisResult.Yes, propertyName)
                            : default;

                    case { ArgumentList: { Arguments: { Count: 1 } arguments } }
                        when arguments[0] is { } argument &&
                             parameter.Type == KnownSymbol.PropertyChangedEventArgs:
                        return PropertyChangedEventArgs.FindPropertyName(argument.Expression, semanticModel, cancellationToken);

                    case { ArgumentList: { Arguments: { Count: 1 } arguments } }
                        when arguments[0] is { Expression: ParenthesizedLambdaExpressionSyntax { Body: { } body } }:
                        return semanticModel.GetSymbolSafe(body, cancellationToken) is { } property
                            ? new AnalysisResult<string?>(AnalysisResult.Yes, property.Name)
                            : default;
                    default:
                        return default;
                }
            }

            return default;
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
