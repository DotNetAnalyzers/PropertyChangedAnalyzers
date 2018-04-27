namespace PropertyChangedAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class InvocationExpressionSyntaxExt
    {
        internal static bool IsPotentialThisOrBase(this InvocationExpressionSyntax invocation)
        {
            switch (invocation.Expression)
            {
                case IdentifierNameSyntax _:
                    return true;
                case MemberAccessExpressionSyntax memberAccess when memberAccess.Expression is InstanceExpressionSyntax:
                    return true;
            }

            return false;
        }

        internal static bool TryGetInvokedSymbol(this InvocationExpressionSyntax invocation, QualifiedMethod expected, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol result)
        {
            result = null;
            if (invocation == null)
            {
                return false;
            }

            if (invocation.TryGetMethodName(out var name) &&
                name != expected.Name)
            {
                return false;
            }

            if (SemanticModelExt.GetSymbolSafe(semanticModel, invocation, cancellationToken) is IMethodSymbol candidate &&
                candidate == expected)
            {
                result = candidate;
                return true;
            }

            return false;
        }

        internal static bool IsNameOf(this InvocationExpressionSyntax invocation)
        {
            return invocation.TryGetMethodName(out var name) &&
                   name == "nameof";
        }
    }
}
