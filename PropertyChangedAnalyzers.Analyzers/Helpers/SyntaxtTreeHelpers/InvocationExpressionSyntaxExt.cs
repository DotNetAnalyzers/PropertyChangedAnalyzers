namespace PropertyChangedAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
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

        internal static bool IsNameOf(this InvocationExpressionSyntax invocation)
        {
            return invocation.TryGetMethodName(out var name) &&
                   name == "nameof";
        }
    }
}
