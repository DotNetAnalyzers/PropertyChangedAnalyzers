namespace PropertyChangedAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Getter
    {
        internal static bool TrySingleReturned(AccessorDeclarationSyntax getter, [NotNullWhen(true)] out ExpressionSyntax? result)
        {
            switch (getter)
            {
                case { ExpressionBody: { Expression: { } expression } }:
                    result = expression;
                    return true;
                case { Body: { Statements: { Count: 1 } statements } }
                    when statements[0] is ReturnStatementSyntax returnStatement:
                    result = returnStatement.Expression;
                    return result != null;
                case { Body: { } body}:
                    return ReturnExpressionsWalker.TryGetSingle(body, out result);
                default:
                    result = null;
                    return false;
            }
        }
    }
}
