namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class SyntaxNodeExt
    {
        internal static bool IsEitherKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2) => node.IsKind(kind1) || node.IsKind(kind2);

        internal static T FirstAncestor<T>(this SyntaxNode node)
            where T : SyntaxNode
        {
            if (node == null)
            {
                return null;
            }

            if (node is T)
            {
                return node.Parent?.FirstAncestorOrSelf<T>();
            }

            return node.FirstAncestorOrSelf<T>();
        }

        internal static bool? IsBeforeInScope(this SyntaxNode node, SyntaxNode other)
        {
            var statement = node?.FirstAncestorOrSelf<StatementSyntax>();
            var otherStatement = other?.FirstAncestorOrSelf<StatementSyntax>();
            if (statement == null ||
                otherStatement == null)
            {
                return null;
            }

            var block = statement.Parent as BlockSyntax;
            var otherBlock = otherStatement.Parent as BlockSyntax;
            if (block == null && otherBlock == null)
            {
                return false;
            }

            if (ReferenceEquals(block, otherBlock) ||
                otherBlock?.Contains(node) == true ||
                block?.Contains(other) == true)
            {
                var firstAnon = FirstAncestor<AnonymousFunctionExpressionSyntax>(node);
                var otherAnon = FirstAncestor<AnonymousFunctionExpressionSyntax>(other);
                if (!ReferenceEquals(firstAnon, otherAnon))
                {
                    return true;
                }

                return statement.SpanStart < otherStatement.SpanStart;
            }

            return false;
        }
    }
}
