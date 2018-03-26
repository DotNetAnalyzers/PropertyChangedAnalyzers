namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal enum Search
    {
        TopLevel,
        Recursive
    }

    internal static class MemberPath
    {
        [Obsolete("Don't use this.", error: true)]
        public static new bool Equals(object x, object y) => throw new InvalidOperationException();

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
                return Equals(xWalker, yWalker);
            }
        }

        internal static bool Equals(PathWalker xWalker, PathWalker yWalker)
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

            return true;
        }

        internal static bool Uses(SyntaxNode scope, PathWalker memberPath, SyntaxNodeAnalysisContext context)
        {
            return UsedMemberWalker.Uses(scope, memberPath, context.ContainingSymbol.ContainingType, context.SemanticModel, context.CancellationToken);
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

            public override string ToString() => string.Join(".", this.identifierNames);

            protected override void Clear()
            {
                this.identifierNames.Clear();
            }
        }

        private sealed class UsedMemberWalker : PooledWalker<UsedMemberWalker>
        {
            private readonly List<ExpressionSyntax> usedMembers = new List<ExpressionSyntax>();
            private readonly List<ExpressionSyntax> recursives = new List<ExpressionSyntax>();
            private readonly HashSet<SyntaxToken> localsAndParameters = new HashSet<SyntaxToken>(SyntaxTokenValueTextComparer.Default);
            private readonly HashSet<SyntaxNode> visited = new HashSet<SyntaxNode>();

            private SemanticModel semanticModel;
            private CancellationToken cancellationToken;
            private ITypeSymbol containingType;

            private UsedMemberWalker()
            {
            }

            public static UsedMemberWalker Borrow(SyntaxNode scope, Search searchOption, ITypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                var pooled = Borrow(() => new UsedMemberWalker());
                pooled.semanticModel = semanticModel;
                pooled.cancellationToken = cancellationToken;
                pooled.containingType = containingType;
                pooled.Visit(scope);
                if (searchOption == Search.Recursive)
                {
                    pooled.VisitRecursive();
                }

                return pooled;
            }

            public static bool Uses(SyntaxNode scope, PathWalker backing, ITypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                using (var walker = Borrow(scope, Search.Recursive, containingType, semanticModel, cancellationToken))
                {
                    foreach (var used in walker.usedMembers)
                    {
                        using (var usedPath = PathWalker.Borrow(used))
                        {
                            if (MemberPath.Equals(usedPath, backing))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            public override void Visit(SyntaxNode node)
            {
                if (this.visited.Add(node))
                {
                    base.Visit(node);
                }
            }

            public override void VisitParameter(ParameterSyntax node)
            {
                this.localsAndParameters.Add(node.Identifier);
            }

            public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                this.localsAndParameters.Add(node.Identifier);
                base.Visit(node.Initializer);
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (!this.localsAndParameters.Contains(node.Identifier))
                {
                    this.usedMembers.Add(node);
                    this.recursives.Add(node);
                }
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                this.usedMembers.Add(node);
                if (node.Expression is InstanceExpressionSyntax)
                {
                    this.recursives.Add(node);
                }
                else
                {
                    base.VisitMemberAccessExpression(node);
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                this.recursives.Add(node);
                base.VisitInvocationExpression(node);
            }

            protected override void Clear()
            {
                this.usedMembers.Clear();
                this.recursives.Clear();
                this.localsAndParameters.Clear();
                this.visited.Clear();
                this.semanticModel = null;
                this.cancellationToken = CancellationToken.None;
                this.containingType = null;
            }

            private void VisitRecursive()
            {
                foreach (var recursive in this.recursives)
                {
                    if (recursive is InvocationExpressionSyntax invocation)
                    {
                        if (this.semanticModel.GetSymbolSafe(invocation, this.cancellationToken) is IMethodSymbol method &&
                            Equals(this.containingType, method.ContainingType) &&
                            method.TrySingleDeclaration(this.cancellationToken, out var declaration))
                        {
                            VisitRecursive((SyntaxNode)declaration.Body ?? declaration.ExpressionBody);
                        }
                    }
                    else if (this.semanticModel.GetSymbolSafe(recursive, this.cancellationToken) is IPropertySymbol property &&
                             Equals(this.containingType, property.ContainingType) &&
                             property.GetMethod.TrySingleDeclaration<AccessorDeclarationSyntax>(this.cancellationToken, out var getter))
                    {
                        VisitRecursive(getter);
                    }
                }

                void VisitRecursive(SyntaxNode body)
                {
                    if (body == null)
                    {
                        return;
                    }

                    using (var walker = Borrow(body, Search.Recursive, this.containingType, this.semanticModel, this.cancellationToken))
                    {
                        walker.visited.UnionWith(this.visited);
                        walker.Visit(body);
                        this.usedMembers.AddRange(walker.usedMembers);
                    }
                }
            }
        }
    }
}
