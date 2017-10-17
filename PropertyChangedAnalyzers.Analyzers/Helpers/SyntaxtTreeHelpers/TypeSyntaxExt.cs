﻿namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeSyntaxExt
    {
        internal static bool IsVoid(this TypeSyntax type)
        {
            if (type is PredefinedTypeSyntax predefinedType)
            {
                return predefinedType.Keyword.ValueText == "void";
            }

            return false;
        }

        internal static bool TryFindField(this TypeDeclarationSyntax type, string name, out FieldDeclarationSyntax match)
        {
            match = null;
            if (type == null)
            {
                return false;
            }

            foreach (var member in type.Members)
            {
                if (member is FieldDeclarationSyntax declaration &&
                    declaration.Declaration.Variables.TryGetSingle(x => x.Identifier.ValueText == name, out _))
                {
                    match = declaration;
                    return true;
                }
            }

            return false;
        }

        internal static bool TryFindMethod(this TypeDeclarationSyntax type, string name, out MethodDeclarationSyntax match)
        {
            match = null;
            if (type == null)
            {
                return false;
            }

            foreach (var member in type.Members)
            {
                if (member is MethodDeclarationSyntax declaration &&
                    declaration.Identifier.ValueText == name)
                {
                    match = declaration;
                    return true;
                }
            }

            return false;
        }

        internal static bool TryFindProperty(this TypeDeclarationSyntax type, string name, out PropertyDeclarationSyntax match)
        {
            match = null;
            if (type == null)
            {
                return false;
            }

            foreach (var member in type.Members)
            {
                if (member is PropertyDeclarationSyntax declaration &&
                    declaration.Identifier.ValueText == name)
                {
                    match = declaration;
                    return true;
                }
            }

            return false;
        }
    }
}