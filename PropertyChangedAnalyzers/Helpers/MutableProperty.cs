namespace PropertyChangedAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal readonly struct MutableProperty
{
    internal readonly AccessorDeclarationSyntax Getter;
    internal readonly AccessorDeclarationSyntax Setter;

    private MutableProperty(AccessorDeclarationSyntax getter, AccessorDeclarationSyntax setter)
    {
        this.Getter = getter;
        this.Setter = setter;
    }

    internal static MutableProperty? Match(PropertyDeclarationSyntax candidate)
    {
        if (candidate.Getter() is { } getter &&
            candidate.Setter() is { } setter)
        {
            return new MutableProperty(getter, setter);
        }

        return null;
    }
}
