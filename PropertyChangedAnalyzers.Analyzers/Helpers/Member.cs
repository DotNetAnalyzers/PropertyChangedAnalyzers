namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Member
    {
        internal static bool IsFieldOrProperty(IdentifierNameSyntax expression)
        {
            return !IdentifierTypeWalker.IsLocalOrParameter(expression);
        }

        internal static bool IsSame(ExpressionSyntax x, ExpressionSyntax y)
        {
            if (TryGetMemberName(x, out var xn))
            {
                if (TryGetMemberName(y, out var yn))
                {
                    return xn == yn;
                }

                return false;
            }

            return false;
        }

        internal static bool TryGetMemberName(ExpressionSyntax ex, out string name)
        {
            name = null;
            if (ex is IdentifierNameSyntax identifierName)
            {
                name = identifierName.Identifier.ValueText;
            }
            else if (ex is MemberAccessExpressionSyntax memberAccess &&
                     memberAccess.Name is SimpleNameSyntax simpleName)
            {
                name = simpleName.Identifier.ValueText;
            }

            return name != null;
        }
    }
}
