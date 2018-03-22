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
            if (TryGetMemberName(x, out var xn, out var subx))
            {
                if (TryGetMemberName(y, out var yn, out var suby))
                {
                    if (xn != yn)
                    {
                        return false;
                    }

                    if (IsRoot(x))
                    {
                        return IsRoot(y);
                    }

                    return IsSame(subx, suby);
                }

                return false;
            }

            return false;
        }

        private static bool TryGetMemberName(ExpressionSyntax expression, out string name, out ExpressionSyntax subExpression)
        {
            name = null;
            subExpression = null;
            switch (expression)
            {
                case IdentifierNameSyntax identifierName:
                    name = identifierName.Identifier.ValueText;
                    break;
                case MemberAccessExpressionSyntax memberAccess:
                    name = memberAccess.Name.Identifier.ValueText;
                    subExpression = memberAccess.Expression;
                    break;
                case MemberBindingExpressionSyntax memberBinding:
                    name = memberBinding.Name.Identifier.ValueText;
                    break;
                case ConditionalAccessExpressionSyntax conditionalAccess:
                    TryGetMemberName(conditionalAccess.WhenNotNull, out name, out _);
                    subExpression = conditionalAccess.Expression;
                    break;
            }

            return name != null;
        }

        private static bool IsRoot(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    return memberAccess.Expression is InstanceExpressionSyntax;
                case IdentifierNameSyntax identifierName:
                    return IsFieldOrProperty(identifierName);
                default:
                    return false;
            }
        }
    }
}
