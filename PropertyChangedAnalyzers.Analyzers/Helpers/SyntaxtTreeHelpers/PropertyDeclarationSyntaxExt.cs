namespace PropertyChangedAnalyzers
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyDeclarationSyntaxExt
    {
        internal static AccessorDeclarationSyntax Getter(this PropertyDeclarationSyntax property)
        {
            if (property.TryGetGetAccessorDeclaration(out var getter))
            {
                return getter;
            }

            throw new InvalidOperationException("Could not find getter, use TryGetGetAccessorDeclaration if you are not sure there is a getter.");
        }

        internal static bool TryGetGetAccessorDeclaration(this PropertyDeclarationSyntax property, out AccessorDeclarationSyntax result)
        {
            return TryGetAccessorDeclaration(property, SyntaxKind.GetAccessorDeclaration, out result);
        }

        internal static AccessorDeclarationSyntax Setter(this PropertyDeclarationSyntax property)
        {
            if (property.TryGetSetAccessorDeclaration(out var getter))
            {
                return getter;
            }

            throw new InvalidOperationException("Could not find getter, use TryGetGetAccessorDeclaration if you are not sure there is a getter.");
        }

        internal static bool TryGetSetAccessorDeclaration(this PropertyDeclarationSyntax property, out AccessorDeclarationSyntax result)
        {
            return TryGetAccessorDeclaration(property, SyntaxKind.SetAccessorDeclaration, out result);
        }

        internal static bool TryGetAccessorDeclaration(this PropertyDeclarationSyntax property, SyntaxKind kind, out AccessorDeclarationSyntax result)
        {
            result = default(AccessorDeclarationSyntax);
            var accessorList = property?.AccessorList;
            if (accessorList == null)
            {
                return false;
            }

            return accessorList.Accessors.TryGetSingle(x => x.IsKind(kind), out result);
        }
    }
}