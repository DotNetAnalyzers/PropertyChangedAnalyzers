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

        internal static bool Uses(ExpressionSyntax assigned, ExpressionSyntax returned, SyntaxNodeAnalysisContext context, PooledHashSet<SyntaxNode> visited = null)
        {
            if (assigned is null ||
                returned is null)
            {
                return false;
            }

            using (var assignedPath = PathWalker.Borrow(assigned))
            {
                var containingType = context.ContainingSymbol.ContainingType;
                if (UsedMemberWalker.Uses(returned, assignedPath, Search.TopLevel, containingType, context.SemanticModel, context.CancellationToken))
                {
                    return true;
                }

                if (assignedPath.IdentifierNames.TrySingle(out var candidate) &&
                    containingType.TryFindPropertyRecursive(candidate.Identifier.ValueText, out var property) &&
                    property.TrySingleDeclaration(context.CancellationToken, out var declaration) &&
                    declaration.TryGetSetter(out var setter) &&
                    Property.TrySingleAssignmentInSetter(setter, out var assignment))
                {
                    using (visited = PooledHashSet<SyntaxNode>.BorrowOrIncrementUsage(visited))
                    {
                        if (visited.Add(candidate))
                        {
                            return Uses(assignment.Left, returned, context, visited);
                        }
                    }
                }
            }

            return false;
        }

        internal static bool Uses(SyntaxNode scope, PathWalker memberPath, SyntaxNodeAnalysisContext context)
        {
            return UsedMemberWalker.Uses(scope, memberPath, Search.Recursive, context.ContainingSymbol.ContainingType, context.SemanticModel, context.CancellationToken);
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

            public static UsedMemberWalker Borrow(SyntaxNode scope, Search searchOption, ITypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken, UsedMemberWalker parent = null)
            {
                var pooled = Borrow(() => new UsedMemberWalker());
                if (parent != null)
                {
                    pooled.visited.UnionWith(parent.visited);
                }

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

            public static bool Uses(SyntaxNode scope, PathWalker backing, Search search, ITypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                using (var walker = Borrow(scope, search, containingType, semanticModel, cancellationToken))
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

            public override void VisitParameter(ParameterSyntax node)
            {
                this.localsAndParameters.Add(node.Identifier);
            }

            public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                this.localsAndParameters.Add(node.Identifier);
                this.Visit(node.Initializer);
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (node.Parent is VariableDeclarationSyntax)
                {
                    return;
                }

                if (!this.localsAndParameters.Contains(node.Identifier))
                {
                    this.usedMembers.Add(node);
                    this.recursives.Add(node);
                }
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                this.usedMembers.Add(node);
                base.VisitMemberAccessExpression(node);

                if (node.Expression is InstanceExpressionSyntax)
                {
                    this.recursives.Add(node);
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.IsPotentialThisOrBase())
                {
                    this.recursives.Add(node);
                }

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
                    if (!this.visited.Add(recursive))
                    {
                        continue;
                    }

                    if (recursive is InvocationExpressionSyntax invocation &&
                        invocation.IsPotentialThisOrBase() &&
                        this.semanticModel.GetSymbolSafe(invocation, this.cancellationToken) is IMethodSymbol method &&
                        Equals(this.containingType, method.ContainingType) &&
                        method.TrySingleDeclaration(this.cancellationToken, out var declaration) &&
                        this.visited.Add(declaration))
                    {
                        VisitRecursive((SyntaxNode)declaration.Body ?? declaration.ExpressionBody);
                    }
                    else if (TryGetProperty(recursive, out var property) &&
                             property.GetMethod.TrySingleDeclaration<AccessorDeclarationSyntax>(this.cancellationToken, out var getter) &&
                             this.visited.Add(getter))
                    {
                        VisitRecursive(getter);
                    }
                }

                bool TryGetProperty(ExpressionSyntax expression, out IPropertySymbol property)
                {
                    if (expression is IdentifierNameSyntax identifierName)
                    {
                        return this.containingType.TryFindProperty(identifierName.Identifier.ValueText, out property) &&
                               property.Equals(this.semanticModel.GetSymbolSafe(identifierName, this.cancellationToken));
                    }

                    if (expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Expression is InstanceExpressionSyntax)
                    {
                        return this.containingType.TryFindProperty(memberAccess.Name.Identifier.ValueText, out property) &&
                               property.Equals(this.semanticModel.GetSymbolSafe(memberAccess.Name, this.cancellationToken));
                    }

                    property = null;
                    return false;
                }

                void VisitRecursive(SyntaxNode body)
                {
                    if (body == null)
                    {
                        return;
                    }

                    using (var walker = Borrow(body, Search.Recursive, this.containingType, this.semanticModel, this.cancellationToken, this))
                    {
                        this.usedMembers.AddRange(walker.usedMembers);
                        this.visited.UnionWith(walker.visited);
                    }
                }
            }
        }
    }
}
