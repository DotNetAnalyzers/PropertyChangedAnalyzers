// ReSharper disable UnusedMember.Global
namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class SymbolExt
    {
        internal static bool IsEither<T1, T2>(this ISymbol symbol)
            where T1 : ISymbol
            where T2 : ISymbol
        {
            return symbol is T1 || symbol is T2;
        }

        internal static bool TryGetSingleDeclaration(this IFieldSymbol field, CancellationToken cancellationToken, out FieldDeclarationSyntax declaration)
        {
            return TryGetSingleDeclaration<FieldDeclarationSyntax>(field, cancellationToken, out declaration);
        }

        internal static bool TrySingleDeclaration(this IPropertySymbol property, CancellationToken cancellationToken, out PropertyDeclarationSyntax declaration)
        {
            return TryGetSingleDeclaration<PropertyDeclarationSyntax>(property, cancellationToken, out declaration);
        }

        internal static bool TrySingleDeclaration(this IMethodSymbol method, CancellationToken cancellationToken, out MethodDeclarationSyntax declaration)
        {
            return TryGetSingleDeclaration<MethodDeclarationSyntax>(method, cancellationToken, out declaration);
        }

        internal static bool TrySingleDeclaration(this IParameterSymbol parameter, CancellationToken cancellationToken, out ParameterSyntax declaration)
        {
            return TryGetSingleDeclaration<ParameterSyntax>(parameter, cancellationToken, out declaration);
        }

        internal static bool TrySingleDeclaration(this ILocalSymbol local, CancellationToken cancellationToken, out VariableDeclarationSyntax declaration)
        {
            return TryGetSingleDeclaration<VariableDeclarationSyntax>(local, cancellationToken, out declaration);
        }

        internal static bool TryGetSingleDeclaration<T>(this ISymbol symbol, CancellationToken cancellationToken, out T declaration)
            where T : SyntaxNode
        {
            declaration = null;
            if (symbol == null)
            {
                return false;
            }

            if (symbol.DeclaringSyntaxReferences.TrySingle(out var reference))
            {
                var syntax = reference.GetSyntax(cancellationToken);
                if (symbol.IsEither<IFieldSymbol, ILocalSymbol>() &&
                    syntax is VariableDeclaratorSyntax declarator)
                {
                    syntax = declarator.FirstAncestor<FieldDeclarationSyntax>();
                }

                declaration = syntax as T;
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
