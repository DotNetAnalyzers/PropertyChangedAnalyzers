namespace PropertyChangedAnalyzers;

using System.Threading;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal readonly struct PropertyChangedEventArgs
{
    internal readonly ArgumentSyntax Argument;

    private PropertyChangedEventArgs(ArgumentSyntax argument)
    {
        this.Argument = argument;
    }

    internal static bool IsCreatedWith(ExpressionSyntax expression, IParameterSymbol parameter, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return Match(expression, semanticModel, cancellationToken) is { Argument: { } argument } &&
               IsMatch(argument);

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

    internal static PropertyChangedEventArgs? Match(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return MatchCreation(expression) ??
               MatchCached(expression, semanticModel, cancellationToken);
    }

    internal PropertyNameArgument? PropertyName(SemanticModel semanticModel, CancellationToken cancellationToken) => PropertyNameArgument.Match(this.Argument, semanticModel, cancellationToken);

    private static PropertyChangedEventArgs? MatchCached(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (semanticModel.TryGetSymbol(expression, cancellationToken, out var candidate))
        {
            switch (candidate)
            {
                case IFieldSymbol field
                    when field.TrySingleDeclaration(cancellationToken, out var declaration) &&
                         declaration.Declaration.Variables.TryLast(out var variable) &&
                         variable.Initializer is { } initializer:
                    return MatchCreation(initializer.Value);
                case IPropertySymbol property
                    when property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax? declaration) &&
                         declaration.Initializer is { } initializer:
                    return MatchCreation(initializer.Value);
                case ILocalSymbol local
                    when local.TrySingleDeclaration(cancellationToken, out VariableDeclarationSyntax? declaration) &&
                         declaration.Variables.TryLast(out var variable) &&
                         variable.Initializer is { } initializer:
                    return MatchCreation(initializer.Value) ??
                           MatchCached(initializer.Value, semanticModel, cancellationToken);
                case IMethodSymbol { Name: "GetOrAdd", ContainingType.TypeArguments: { Length: 2 } typeArguments } method
                    when method.ContainingType == KnownSymbol.ConcurrentDictionaryOfTKeyTValue &&
                         typeArguments[0] == KnownSymbol.String &&
                         typeArguments[1] == KnownSymbol.PropertyChangedEventArgs &&
                         expression is InvocationExpressionSyntax invocation &&
                         invocation.TryFindArgument(method.Parameters[0], out var nameArg):
                    return new PropertyChangedEventArgs(nameArg);
            }
        }

        return null;
    }

    private static PropertyChangedEventArgs? MatchCreation(ExpressionSyntax expression)
    {
        return expression is ObjectCreationExpressionSyntax { Type: { } type, ArgumentList.Arguments: { Count: 1 } arguments } &&
               type == KnownSymbol.PropertyChangedEventArgs &&
               arguments.TrySingle(out var nameArg)
            ? new PropertyChangedEventArgs(nameArg)
            : (PropertyChangedEventArgs?)null;
    }
}
