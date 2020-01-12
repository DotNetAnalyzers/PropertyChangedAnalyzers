namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Getter
    {
        internal static ExpressionSyntax? FindSingleReturned(AccessorDeclarationSyntax getter)
        {
            switch (getter)
            {
                case { ExpressionBody: { Expression: { } expression } }:
                    return expression;
                case { Body: { Statements: { Count: 1 } statements } }
                    when statements[0] is ReturnStatementSyntax returnStatement:
                    return returnStatement.Expression;
                case { Body: { } body }:
                    return ReturnExpressionsWalker.TryGetSingle(body, out var result) ? result : null;
                default:
                    return null;
            }
        }
    }
}
