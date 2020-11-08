namespace PropertyChangedAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyChangedEvent
    {
        internal static IEventSymbol? Find(ITypeSymbol type)
        {
            return type.TryFindEventRecursive("PropertyChanged", out var propertyChangedEvent)
                ? propertyChangedEvent
                : null;
        }

        internal static bool IsInvoke(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (invocation is { ArgumentList: { Arguments: { Count: 2 } arguments } } &&
                arguments[0].Expression.IsEither(SyntaxKind.ThisExpression, SyntaxKind.NullLiteralExpression) &&
                invocation.IsPotentialReturnVoid())
            {
                return invocation.Parent switch
                {
                    ConditionalAccessExpressionSyntax conditionalAccess
                    => IsPotential(conditionalAccess) &&
                       semanticModel.TryGetSymbol(invocation, KnownSymbol.PropertyChangedEventHandler.Invoke, cancellationToken, out _),
                    ExpressionStatementSyntax _
                    => semanticModel.TryGetSymbol(invocation, cancellationToken, out var symbol) &&
                       symbol == KnownSymbol.PropertyChangedEventHandler.Invoke,
                    _ => false,
                };
            }

            return false;

            static bool IsPotential(ConditionalAccessExpressionSyntax candidate)
            {
                return candidate.Expression is IdentifierNameSyntax ||
                       candidate.Expression is MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax _, Name: IdentifierNameSyntax _ };
            }
        }
    }
}
