namespace PropertyChangedAnalyzers
{
    using System;
    using Gu.Roslyn.AnalyzerExtensions;
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

        internal static AccessorDeclarationSyntax Setter(this PropertyDeclarationSyntax property)
        {
            if (property.TryGetSetter(out var getter))
            {
                return getter;
            }

            throw new InvalidOperationException("Could not find getter, use TryGetGetter if you are not sure there is a getter.");
        }
    }
}
