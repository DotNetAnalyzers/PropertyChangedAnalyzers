namespace PropertyChangedAnalyzers
{
    using System;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyDeclarationSyntaxExt
    {
        internal static AccessorDeclarationSyntax Getter(this PropertyDeclarationSyntax property)
        {
            if (property.TryGetGetter(out var getter))
            {
                return getter;
            }

            throw new InvalidOperationException("Could not find getter, use TryGetGetter if you are not sure there is a getter.");
        }

        internal static bool TryGetGetter(this PropertyDeclarationSyntax property, out AccessorDeclarationSyntax result)
        {
            result = null;
            return property?.AccessorList?.Accessors.TryFirst(x => x.IsKind(SyntaxKind.GetAccessorDeclaration), out result) == true;
        }

        internal static AccessorDeclarationSyntax Setter(this PropertyDeclarationSyntax property)
        {
            if (property.TryGetSetter(out var getter))
            {
                return getter;
            }

            throw new InvalidOperationException("Could not find getter, use TryGetGetter if you are not sure there is a getter.");
        }

        internal static bool TryGetSetter(this PropertyDeclarationSyntax property, out AccessorDeclarationSyntax result)
        {
            result = null;
            return property?.AccessorList?.Accessors.TryFirst(x => x.IsKind(SyntaxKind.SetAccessorDeclaration), out result) == true;
        }

        internal static bool TryGetAccessorDeclaration(this PropertyDeclarationSyntax property, SyntaxKind kind, out AccessorDeclarationSyntax result)
        {
            result = default(AccessorDeclarationSyntax);
            var accessorList = property?.AccessorList;
            if (accessorList == null)
            {
                return false;
            }

            return accessorList.Accessors.TrySingle(x => x.IsKind(kind), out result);
        }
    }
}
