namespace PropertyChangedAnalyzers
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyDeclarationSyntaxExt
    {
        internal static bool TryGetGetAccessorDeclaration(this PropertyDeclarationSyntax property, out AccessorDeclarationSyntax result)
        {
            return TryGetAccessorDeclaration(property, SyntaxKind.GetAccessorDeclaration, out result);
        }

        internal static bool TryGetSetAccessorDeclaration(this PropertyDeclarationSyntax property, out AccessorDeclarationSyntax result)
        {
            return TryGetAccessorDeclaration(property, SyntaxKind.SetAccessorDeclaration, out result);
        }

        internal static bool TryGetAccessorDeclaration(this PropertyDeclarationSyntax property, SyntaxKind kind, out AccessorDeclarationSyntax result)
        {
            result = null;
            var accessors = property?.AccessorList?.Accessors;
            if (accessors == null)
            {
                return false;
            }

            foreach (var accessor in accessors.Value)
            {
                if (accessor.IsKind(kind))
                {
                    result = accessor;
                    return true;
                }
            }

            if (accessors.Value.Count == 1 &&
                ((CSharpParseOptions)property.SyntaxTree.Options).LanguageVersion >= LanguageVersion.CSharp6)
            {
                var node = accessors.Value[0];
                throw new NotImplementedException();
               //if( node.DescendantNodes(x=> x.IsKind(SyntaxKind.GetKeyword)).TryGetFirst(out var get)
               // {
                    
               // }
               // if (node.ChildNodes().TryGetSingle(out SyntaxNode c1) &&
               //     c1.ChildNodes().TryGetAtIndex(1, out setter))
               // {
                    
               // }
            }

            return false;
        }
    }
}