namespace PropertyChangedAnalyzers
{
    using System.Runtime.CompilerServices;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// The safe versions handle situations like partial classes when the node is not in the same syntax tree.
    /// </summary>
    internal static class SemanticModelExt
    {
        private static readonly ConditionalWeakTable<SyntaxTree, SemanticModel> Cache = new ConditionalWeakTable<SyntaxTree, SemanticModel>();

        internal static ISymbol GetSymbolSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            return SemanticModelFor(semanticModel, node)?.GetSymbolInfo(node, cancellationToken).Symbol;
        }

        internal static IFieldSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, FieldDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (IFieldSymbol)GetDeclaredSymbolSafe(semanticModel, (SyntaxNode)node, cancellationToken);
        }

        internal static IMethodSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, ConstructorDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (IMethodSymbol)GetDeclaredSymbolSafe(semanticModel, (SyntaxNode)node, cancellationToken);
        }

        internal static IPropertySymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, PropertyDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (IPropertySymbol)GetDeclaredSymbolSafe(semanticModel, (SyntaxNode)node, cancellationToken);
        }

        internal static IMethodSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, MethodDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (IMethodSymbol)GetDeclaredSymbolSafe(semanticModel, (SyntaxNode)node, cancellationToken);
        }

        internal static ITypeSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, TypeDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (ITypeSymbol)GetDeclaredSymbolSafe(semanticModel, (SyntaxNode)node, cancellationToken);
        }

        internal static ISymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            return SemanticModelFor(semanticModel, node)?.GetDeclaredSymbol(node, cancellationToken);
        }

        internal static Optional<object> GetConstantValueSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            return SemanticModelFor(semanticModel, node)?.GetConstantValue(node, cancellationToken) ?? default(Optional<object>);
        }

        internal static bool TryGetConstantValue<T>(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken, out T value)
        {
            var optional = GetConstantValueSafe(semanticModel, node, cancellationToken);
            if (optional.HasValue)
            {
                value = (T)optional.Value;
                return true;
            }

            value = default(T);
            return false;
        }

        internal static TypeInfo GetTypeInfoSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            return SemanticModelFor(semanticModel, node)?.GetTypeInfo(node, cancellationToken) ?? default(TypeInfo);
        }

        /// <summary>
        /// Gets the semantic model for <paramref name="node"/>
        /// This can be needed for partial classes.
        /// </summary>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="node">The expression.</param>
        /// <returns>The semantic model that corresponds to <paramref name="node"/></returns>
        internal static SemanticModel SemanticModelFor(this SemanticModel semanticModel, SyntaxNode node)
        {
            SemanticModel Create(Compilation compilation, SyntaxTree tree)
            {
                if (compilation.ContainsSyntaxTree(tree))
                {
                    return semanticModel.Compilation.GetSemanticModel(tree);
                }

                foreach (var reference in compilation.References)
                {
                    if (reference is CompilationReference compilationReference)
                    {
                        if (compilationReference.Compilation.ContainsSyntaxTree(tree))
                        {
                            return compilationReference.Compilation.GetSemanticModel(tree);
                        }
                    }
                }

                return null;
            }

            if (semanticModel == null ||
                node == null ||
                node.SyntaxTree == null ||
                node.IsMissing)
            {
                return null;
            }

            return Cache.GetValue(node.SyntaxTree, x => Create(semanticModel.Compilation, x));
        }
    }
}