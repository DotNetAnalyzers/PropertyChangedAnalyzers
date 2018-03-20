namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    internal static class SyntaxTokenListExt
    {
        internal static bool Any(this SyntaxTokenList list, SyntaxKind kind1, SyntaxKind kind2) => list.Any(kind1) || list.Any(kind2);
    }
}