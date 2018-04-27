namespace PropertyChangedAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class IfStatementSyntaxExt
    {
        internal static bool IsReturnOnly(this IfStatementSyntax ifStatement)
        {
            if (ifStatement == null)
            {
                return false;
            }

            if (ifStatement.Statement is ReturnStatementSyntax)
            {
                return true;
            }

            if (ifStatement.Statement is BlockSyntax block &&
                block.Statements.TrySingle(out var statement) &&
                statement is ReturnStatementSyntax)
            {
                return true;
            }

            return false;
        }

        internal static bool IsReturnIfTrue(this IfStatementSyntax ifStatement)
        {
            if (ifStatement == null)
            {
                return false;
            }

            if (ifStatement.Statement is ReturnStatementSyntax)
            {
                return true;
            }

            if (ifStatement.Statement is BlockSyntax block)
            {
                foreach (var statement in block.Statements)
                {
                    if (statement is ReturnStatementSyntax)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
