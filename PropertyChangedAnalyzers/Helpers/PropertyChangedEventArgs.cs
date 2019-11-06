namespace PropertyChangedAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyChangedEventArgs
    {
        internal static bool IsCreatedWith(ExpressionSyntax expression, IParameterSymbol parameter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (TryGetCached(expression, semanticModel, cancellationToken, out var argument))
            {
                return IsMatch(argument);
            }

            if (expression is ObjectCreationExpressionSyntax)
            {
                return parameter.Type == KnownSymbol.String &&
                       TryGetCreation(expression, out argument) &&
                       IsMatch(argument);
            }

            return false;

            bool IsMatch(ArgumentSyntax candidate)
            {
                return candidate.Expression switch
                {
                    IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText == parameter.Name,
                    BinaryExpressionSyntax { Left: IdentifierNameSyntax left } binary => binary.IsKind(SyntaxKind.CoalesceExpression) &&
                                                                                         left.Identifier.ValueText == parameter.Name,
                    _ => false,
                };
            }
        }

        internal static bool TryGetPropertyName(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out string? propertyName)
        {
            if (TryGetCreation(expression, out var nameArgument) ||
                TryGetCached(expression, semanticModel, cancellationToken, out nameArgument))
            {
                return nameArgument.TryGetStringValue(semanticModel, cancellationToken, out propertyName);
            }

            propertyName = null;
            return false;
        }

        internal static bool TryGetPropertyNameArgument(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ArgumentSyntax? nameArgument)
        {
            return TryGetCreation(expression, out nameArgument) ||
                   TryGetCached(expression, semanticModel, cancellationToken, out nameArgument);
        }

        private static bool TryGetCached(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ArgumentSyntax? nameArg)
        {
            if (semanticModel.TryGetSymbol(expression, cancellationToken, out var candidate))
            {
                switch (candidate)
                {
                    case IFieldSymbol field when field.TrySingleDeclaration(cancellationToken, out var declaration) &&
                                                 declaration.Declaration.Variables.TryLast(out var variable) &&
                                                 variable.Initializer is { } initializer:
                        return TryGetCreation(initializer.Value, out nameArg);
                    case IPropertySymbol property when property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax? declaration) &&
                                                       declaration.Initializer is { } initializer:
                        return TryGetCreation(initializer.Value, out nameArg);
                    case ILocalSymbol local when local.TrySingleDeclaration(cancellationToken, out VariableDeclarationSyntax? declaration) &&
                                                 declaration.Variables.TryLast(out var variable) &&
                                                 variable.Initializer is { } initializer:
                        return TryGetCreation(initializer.Value, out nameArg) ||
                               TryGetCached(initializer.Value, semanticModel, cancellationToken, out nameArg);
                    case IMethodSymbol { Name: "GetOrAdd", ContainingType: { TypeArguments: { Length: 2 } typeArguments } } method
                        when method.ContainingType == KnownSymbol.ConcurrentDictionaryOfTKeyTValue &&
                             typeArguments[0] == KnownSymbol.String &&
                             typeArguments[1] == KnownSymbol.PropertyChangedEventArgs &&
                             expression is InvocationExpressionSyntax invocation &&
                             invocation.TryFindArgument(method.Parameters[0], out nameArg):
                        return true;
                }
            }

            nameArg = null;
            return false;
        }

        private static bool TryGetCreation(this ExpressionSyntax expression, [NotNullWhen(true)] out ArgumentSyntax? nameArg)
        {
            nameArg = null;
            return expression is ObjectCreationExpressionSyntax { Type: { } type, ArgumentList: { Arguments: { Count: 1 } arguments } } &&
                   type == KnownSymbol.PropertyChangedEventArgs &&
                   arguments.TrySingle(out nameArg);
        }
    }
}
