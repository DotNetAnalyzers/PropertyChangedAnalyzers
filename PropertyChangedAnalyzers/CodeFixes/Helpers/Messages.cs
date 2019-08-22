namespace PropertyChangedAnalyzers
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static class Messages
    {
        internal static string DisplaySignature(this IMethodSymbol method)
        {
            return $"{method.Name}({string.Join(", ", method.Parameters.Where(x => !(x.IsOptional && x.Type.Kind != SymbolKind.TypeParameter)).Select(x => $"{(x.RefKind == RefKind.Ref ? "ref " : string.Empty)}{x.Name}"))})";
        }
    }
}
