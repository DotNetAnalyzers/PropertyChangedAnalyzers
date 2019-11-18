namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class PropertyPath
    {
        [Obsolete("Don't use this.", error: true)]
#pragma warning disable GU0073 // Member of non-public type should be internal.
        public static new bool Equals(object x, object y) => throw new InvalidOperationException();
#pragma warning restore GU0073 // Member of non-public type should be internal.

        internal static bool Uses(ExpressionSyntax assigned, ExpressionSyntax returned, SyntaxNodeAnalysisContext context, PooledSet<SyntaxNode>? visited = null)
        {
            if (assigned is null ||
                returned is null)
            {
                return false;
            }

            using (var assignedPath = MemberPath.PathWalker.Borrow(assigned))
            {
                var containingType = context.ContainingSymbol.ContainingType;
                if (UsedMemberWalker.Uses(returned, assignedPath, Search.TopLevel, containingType, context.SemanticModel, context.CancellationToken))
                {
                    return true;
                }

                if (assignedPath.Tokens.TrySingle(out var candidate) &&
                    containingType.TryFindPropertyRecursive(candidate.ValueText, out var property) &&
                    property.TrySingleDeclaration(context.CancellationToken, out var declaration) &&
                    declaration.TryGetSetter(out var setter) &&
                    Setter.TryFindSingleAssignment(setter, out var assignment))
                {
                    using (var set = visited.IncrementUsage())
                    {
                        if (set.Add(candidate.Parent))
                        {
                            return Uses(assignment.Left, returned, context, set);
                        }
                    }
                }
            }

            return false;
        }

        internal static bool Uses(SyntaxNode scope, MemberPath.PathWalker memberPath, SyntaxNodeAnalysisContext context)
        {
            return UsedMemberWalker.Uses(scope, memberPath, Search.Recursive, context.ContainingSymbol.ContainingType, context.SemanticModel, context.CancellationToken);
        }

        private static bool Equals(MemberPath.PathWalker xWalker, MemberPath.PathWalker yWalker)
        {
            var xPath = xWalker.Tokens;
            var yPath = yWalker.Tokens;
            if (xPath.Count == 0 ||
                xPath.Count != yPath.Count)
            {
                return false;
            }

            for (var i = 0; i < xPath.Count; i++)
            {
                if (xPath[i].ValueText != yPath[i].ValueText)
                {
                    return false;
                }
            }

            return true;
        }

        private sealed class UsedMemberWalker : PooledWalker<UsedMemberWalker>
        {
            private readonly List<ExpressionSyntax> usedMembers = new List<ExpressionSyntax>();
            private readonly List<ExpressionSyntax> recursives = new List<ExpressionSyntax>();
            private readonly HashSet<SyntaxToken> localsAndParameters = new HashSet<SyntaxToken>(SyntaxTokenComparer.ByValueText);
            private readonly HashSet<SyntaxNode> visited = new HashSet<SyntaxNode>();

            private SemanticModel semanticModel = null!;
            private CancellationToken cancellationToken;
            private ITypeSymbol containingType = null!;

            private UsedMemberWalker()
            {
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

            internal static UsedMemberWalker Borrow(SyntaxNode scope, Search searchOption, ITypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken, UsedMemberWalker? parent = null)
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

            internal static bool Uses(SyntaxNode scope, MemberPath.PathWalker backing, Search search, ITypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                using (var walker = Borrow(scope, search, containingType, semanticModel, cancellationToken))
                {
                    foreach (var used in walker.usedMembers)
                    {
                        using var usedPath = MemberPath.PathWalker.Borrow(used);
                        if (PropertyPath.Equals(usedPath, backing))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            protected override void Clear()
            {
                this.usedMembers.Clear();
                this.recursives.Clear();
                this.localsAndParameters.Clear();
                this.visited.Clear();
                this.semanticModel = null!;
                this.cancellationToken = CancellationToken.None;
                this.containingType = null!;
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
                        this.semanticModel.GetSymbolSafe(invocation, this.cancellationToken) is { } method &&
                        Equals(this.containingType, method.ContainingType) &&
                        method.TrySingleDeclaration(this.cancellationToken, out MethodDeclarationSyntax? declaration) &&
                        this.visited.Add(declaration))
                    {
                        VisitRecursive((SyntaxNode)declaration.Body ?? declaration.ExpressionBody);
                    }
                    else if (TryGetProperty(recursive, out var property) &&
                             property.GetMethod is { } getMethod &&
                             getMethod.TrySingleDeclaration(this.cancellationToken, out SyntaxNode? getter) &&
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

                    using var walker = Borrow(body, Search.Recursive, this.containingType, this.semanticModel, this.cancellationToken, this);
                    this.usedMembers.AddRange(walker.usedMembers);
                    this.visited.UnionWith(walker.visited);
                }
            }
        }
    }
}
