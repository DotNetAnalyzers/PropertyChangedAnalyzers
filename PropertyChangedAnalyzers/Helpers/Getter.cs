namespace PropertyChangedAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Getter
    {
        internal static bool TrySingleReturned(PropertyDeclarationSyntax property, out ExpressionSyntax result)
        {
            if (property == null)
            {
                result = null;
                return false;
            }

            if (property.ExpressionBody is ArrowExpressionClauseSyntax expressionBody)
            {
                result = expressionBody.Expression;
                return result != null;
            }

            result = null;
            return property.TryGetGetter(out var getter) &&
                   TrySingleReturned(getter, out result);
        }

        internal static bool TrySingleReturned(AccessorDeclarationSyntax getter, out ExpressionSyntax result)
        {
            if (getter.ExpressionBody is ArrowExpressionClauseSyntax getterExpressionBody)
            {
                result = getterExpressionBody.Expression;
                return result != null;
            }

            if (getter.Body is BlockSyntax body)
            {
                if (body.Statements.Count == 0)
                {
                    result = null;
                    return false;
                }

                if (body.Statements.TrySingle(out var statement))
                {
                    if (statement is ReturnStatementSyntax returnStatement)
                    {
                        result = returnStatement.Expression;
                        return result != null;
                    }
                }

                return ReturnExpressionsWalker.TryGetSingle(getter, out result);
            }

            result = null;
            return false;
        }
    }
}
