namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

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
    }
}
