namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MemberPath
    {
        internal static bool Equals(ExpressionSyntax x, ExpressionSyntax y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null ||
                y is null)
            {
                return false;
            }

            using (var xWalker = PathWalker.Borrow(x))
            using (var yWalker = PathWalker.Borrow(y))
            {
                var xPath = xWalker.IdentifierNames;
                var yPath = yWalker.IdentifierNames;
                if (xPath.Count == 0 ||
                    xPath.Count != yPath.Count)
                {
                    return false;
                }

                for (var i = 0; i < xPath.Count; i++)
                {
                    if (xPath[i].Identifier.ValueText != yPath[i].Identifier.ValueText)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal static bool TryGetMemberName(ExpressionSyntax expression, out string name)
        {
            name = null;
            switch (expression)
            {
                case IdentifierNameSyntax identifierName:
                    name = identifierName.Identifier.ValueText;
                    break;
                case MemberAccessExpressionSyntax memberAccess:
                    name = memberAccess.Name.Identifier.ValueText;
                    break;
                case MemberBindingExpressionSyntax memberBinding:
                    name = memberBinding.Name.Identifier.ValueText;
                    break;
                case ConditionalAccessExpressionSyntax conditionalAccess:
                    TryGetMemberName(conditionalAccess.WhenNotNull, out name);
                    break;
            }

            return name != null;
        }

        internal sealed class PathWalker : PooledWalker<PathWalker>
        {
            private readonly List<IdentifierNameSyntax> identifierNames = new List<IdentifierNameSyntax>();

            private PathWalker()
            {
            }

            public IReadOnlyList<IdentifierNameSyntax> IdentifierNames => this.identifierNames;

            public static PathWalker Borrow(ExpressionSyntax node)
            {
                var walker = BorrowAndVisit(node, () => new PathWalker());
                if (walker.identifierNames.TryFirst(out var first))
                {
                    if (first.Parent is InstanceExpressionSyntax)
                    {
                        return walker;
                    }

                    if (IdentifierTypeWalker.IsLocalOrParameter(first))
                    {
                        walker.identifierNames.Clear();
                    }
                }

                return walker;
            }

            public override void Visit(SyntaxNode node)
            {
                switch (node.Kind())
                {
                    case SyntaxKind.ConditionalAccessExpression:
                    case SyntaxKind.SimpleMemberAccessExpression:
                    case SyntaxKind.MemberBindingExpression:
                    case SyntaxKind.IdentifierName:
                        base.Visit(node);
                        break;
                }
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                this.identifierNames.Add(node);
            }

            protected override void Clear()
            {
                this.identifierNames.Clear();
            }
        }
    }
}
