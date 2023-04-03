namespace PropertyChangedAnalyzers;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class IfStatementSyntaxExt
{
    internal static bool IsReturnOnly(this IfStatementSyntax ifStatement)
    {
        return ifStatement.Statement switch
        {
            ReturnStatementSyntax _ => true,
            BlockSyntax { Statements: { } statements }
                => statements.Last() is ReturnStatementSyntax,
            _ => false,
        };
    }
}
