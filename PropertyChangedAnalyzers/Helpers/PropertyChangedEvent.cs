namespace PropertyChangedAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyChangedEvent
    {
        internal static bool TryFind(ITypeSymbol type, [NotNullWhen(true)] out IEventSymbol? propertyChangedEvent)
        {
            return type.TryFindEventRecursive("PropertyChanged", out propertyChangedEvent);
        }

        internal static bool IsInvoke(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (invocation.ArgumentList?.Arguments.Count == 2 &&
                invocation.ArgumentList.Arguments[0].Expression.IsEither(SyntaxKind.ThisExpression, SyntaxKind.NullLiteralExpression) &&
                invocation.IsPotentialReturnVoid())
            {
                switch (invocation.Parent)
                {
                    case ConditionalAccessExpressionSyntax conditionalAccess when IsPotential(conditionalAccess):
                        return semanticModel.TryGetSymbol(invocation, KnownSymbol.PropertyChangedEventHandler.Invoke, cancellationToken, out _);
                    case ExpressionStatementSyntax _ when semanticModel.TryGetSymbol(invocation, cancellationToken, out var symbol):
                        return symbol == KnownSymbol.PropertyChangedEventHandler.Invoke;
                    default:
                        return false;
                }
            }

            return false;

            bool IsPotential(ConditionalAccessExpressionSyntax candidate)
            {
                return candidate.Expression is IdentifierNameSyntax ||
                       (candidate.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Expression is ThisExpressionSyntax);
            }
        }
    }
}
