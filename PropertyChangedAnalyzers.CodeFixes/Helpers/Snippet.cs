namespace PropertyChangedAnalyzers.Helpers
{
    using Microsoft.CodeAnalysis;

    internal static class Snippet
    {
        internal static string EqualityCheck(ITypeSymbol type, string x, string y, SemanticModel semanticModel)
        {
            if (type == KnownSymbol.String)
            {
                return $"{x} == {y}";
            }

            if (!type.IsReferenceType)
            {
                if (Equality.HasEqualityOperator(type))
                {
                    return $"{x} == {y}";
                }

                if (type == KnownSymbol.NullableOfT)
                {
                    return Equality.HasEqualityOperator(((INamedTypeSymbol)type).TypeArguments[0])
                        ? $"{x} == {y}"
                        : $"System.Nullable.Equals({x}, {y})";
                }

                if (type.GetMembers("Equals")
                        .OfType<IMethodSymbol>()
                        .TryGetSingle(
                            m => m.Parameters.Length == 1 &&
                                 ReferenceEquals(type, m.Parameters[0].Type), out _))
                {
                    return $"{x}.Equals({y})";
                }

                return $"System.Collections.Generic.EqualityComparer<{type.ToDisplayString()}>.Default.Equals({x}, {y})";
            }

            if (semanticModel.IsUseReferenceEqualsSuppressed())
            {
                return $"Equals({x}, {y})";
            }

            return $"ReferenceEquals({x}, {y})";
        }
    }
}
