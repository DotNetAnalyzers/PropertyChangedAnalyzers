namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class SymbolExt
    {
        internal static bool TryGetSingleDeclaration(this IPropertySymbol property, CancellationToken cancellationToken, out PropertyDeclarationSyntax declaration)
        {
            return TryGetSingleDeclaration<PropertyDeclarationSyntax>(property, cancellationToken, out declaration);
        }

        internal static bool TryGetSingleDeclaration(this IMethodSymbol method, CancellationToken cancellationToken, out MethodDeclarationSyntax declaration)
        {
            return TryGetSingleDeclaration<MethodDeclarationSyntax>(method, cancellationToken, out declaration);
        }

        internal static bool TryGetSingleDeclaration<T>(this ISymbol symbol, CancellationToken cancellationToken, out T declaration)
            where T : SyntaxNode
        {
            declaration = null;
            if (symbol == null)
            {
                return false;
            }

            if (symbol.DeclaringSyntaxReferences.TryGetSingle(out var syntaxReference))
            {
                declaration = syntaxReference.GetSyntax(cancellationToken) as T;
                return declaration != null;
            }

            return false;
        }

        internal static IEnumerable<SyntaxNode> Declarations(this ISymbol symbol, CancellationToken cancellationToken)
        {
            foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
            {
                yield return syntaxReference.GetSyntax(cancellationToken);
            }
        }
    }
}