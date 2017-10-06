namespace PropertyChangedAnalyzers.Test
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class SyntaxNodeExt
    {
        internal static PropertyDeclarationSyntax PropertyDeclarationSyntax(this SyntaxTree tree, string code)
        {
            return tree.BestMatch<PropertyDeclarationSyntax>(code);
        }

        internal static T BestMatch<T>(this SyntaxTree tree, string code)
            where T : SyntaxNode
        {
            SyntaxNode parent = null;
            T best = null;
            foreach (var node in tree.GetRoot()
                                     .DescendantNodes()
                                     .OfType<T>())
            {
                var statementSyntax = node.FirstAncestorOrSelf<StatementSyntax>();
                if (statementSyntax?.ToFullString().Contains(code) == true)
                {
                    if (parent == null || statementSyntax.Span.Length < parent.Span.Length)
                    {
                        parent = statementSyntax;
                        best = node;
                    }
                }

                var member = node.FirstAncestorOrSelf<MemberDeclarationSyntax>();
                if (member?.ToFullString().Contains(code) == true)
                {
                    if (parent == null || member.Span.Length < parent.Span.Length)
                    {
                        parent = member;
                        best = node;
                    }
                }
            }

            if (best == null)
            {
                throw new InvalidOperationException($"The tree does not contain an {typeof(T).Name} matching the code.");
            }

            return best;
        }
    }
}