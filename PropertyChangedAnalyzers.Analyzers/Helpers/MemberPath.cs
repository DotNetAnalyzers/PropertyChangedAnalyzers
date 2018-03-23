namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

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

        internal static bool Uses(SyntaxNode scope, MemberPath.PathWalker memberPath, SyntaxNodeAnalysisContext context)
        {
            using (var walker = UsedMemberWalker.Borrow(scope, context.ContainingSymbol.ContainingType, context.SemanticModel, context.CancellationToken))
            {
                return walker.Uses(memberPath);
            }
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
            private readonly List<ExpressionSyntax> useds = new List<ExpressionSyntax>();
            private readonly HashSet<SyntaxToken> localsAndParameters = new HashSet<SyntaxToken>(SyntaxTokenValueTextComparer.Default);
            private readonly HashSet<SyntaxNode> visited = new HashSet<SyntaxNode>();

            private SemanticModel semanticModel;
            private CancellationToken cancellationToken;
            private ITypeSymbol containingType;

            private UsedMemberWalker()
            {
            }

            public static UsedMemberWalker Borrow(SyntaxNode scope, ITypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                var pooled = Borrow(() => new UsedMemberWalker());
                pooled.semanticModel = semanticModel;
                pooled.cancellationToken = cancellationToken;
                pooled.containingType = containingType;
                pooled.Visit(scope);
                return pooled;
            }

            public bool Uses(MemberPath.PathWalker backing)
            {
                foreach (var used in this.useds)
                {
                    using (var usedPath = MemberPath.PathWalker.Borrow(used))
                    {
                        if (MemberPath.Equals(usedPath, backing))
                        {
                            return true;
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
                    this.useds.Add(node);
                    this.VisitRecursive(this.semanticModel.GetSymbolSafe(node, this.cancellationToken) as IPropertySymbol);
                }
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                this.useds.Add(node);
                if (node.Expression is InstanceExpressionSyntax)
                {
                    this.VisitRecursive(this.semanticModel.GetSymbolSafe(node, this.cancellationToken) as IPropertySymbol);
                }
                else
                {
                    base.VisitMemberAccessExpression(node);
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (this.semanticModel.GetSymbolSafe(node, this.cancellationToken) is IMethodSymbol method &&
                    Equals(this.containingType, method.ContainingType) &&
                    method.TrySingleDeclaration(this.cancellationToken, out var declaration))
                {
                    this.VisitRecursive((SyntaxNode)declaration.Body ?? declaration.ExpressionBody);
                }

                base.VisitInvocationExpression(node);
            }

            protected override void Clear()
            {
                this.useds.Clear();
                this.localsAndParameters.Clear();
                this.visited.Clear();
                this.semanticModel = null;
                this.cancellationToken = CancellationToken.None;
                this.containingType = null;
            }

            private void VisitRecursive(IPropertySymbol property)
            {
                if (Equals(this.containingType, property.ContainingType) &&
                    property.GetMethod.TrySingleDeclaration(this.cancellationToken, out var getter))
                {
                    this.VisitRecursive(getter);
                }
            }

            private void VisitRecursive(SyntaxNode body)
            {
                if (body == null)
                {
                    return;
                }

                using (var walker = Borrow(body, this.containingType, this.semanticModel, this.cancellationToken))
                {
                    walker.visited.UnionWith(this.visited);
                    walker.Visit(body);
                    this.useds.AddRange(walker.useds);
                }
            }
        }
    }
}
