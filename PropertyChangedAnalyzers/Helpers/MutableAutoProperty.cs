namespace PropertyChangedAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal readonly struct MutableAutoProperty
{
    internal readonly AccessorDeclarationSyntax Getter;
    internal readonly AccessorDeclarationSyntax Setter;

    private MutableAutoProperty(AccessorDeclarationSyntax getter, AccessorDeclarationSyntax setter)
    {
        this.Getter = getter;
        this.Setter = setter;
    }

    internal static MutableAutoProperty? Match(PropertyDeclarationSyntax candidate)
    {
        if (candidate.Getter() is { ExpressionBody: null, Body: null } getter &&
            candidate.Setter() is { ExpressionBody: null, Body: null } setter)
        {
            return new MutableAutoProperty(getter, setter);
        }

        return null;
    }
}
