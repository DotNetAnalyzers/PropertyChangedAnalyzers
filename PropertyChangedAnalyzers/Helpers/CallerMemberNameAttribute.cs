namespace PropertyChangedAnalyzers;

using Microsoft.CodeAnalysis;

internal static class CallerMemberNameAttribute
{
    internal static bool IsAvailable(SemanticModel semanticModel)
    {
        return semanticModel.Compilation.GetTypeByMetadataName(KnownSymbol.CallerMemberNameAttribute.FullName) != null;
    }
}
