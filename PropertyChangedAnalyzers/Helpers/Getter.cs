namespace PropertyChangedAnalyzers;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class Getter
{
    internal static ExpressionSyntax? FindSingleReturned(AccessorDeclarationSyntax getter)
    {
        return getter switch
        {
            { ExpressionBody: { Expression: { } expression } } => expression,
            { Body: { Statements: { Count: 1 } statements } }
                when statements[0] is ReturnStatementSyntax returnStatement
                => returnStatement.Expression,
            { Body: { } body } => ReturnExpressionsWalker.TryGetSingle(body, out var result) ? result : null,
            _ => null,
        };
    }
}
