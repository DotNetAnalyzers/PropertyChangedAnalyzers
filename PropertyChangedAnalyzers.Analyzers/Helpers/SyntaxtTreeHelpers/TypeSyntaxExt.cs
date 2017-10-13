namespace PropertyChangedAnalyzers
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
    }
}