namespace PropertyChangedAnalyzers
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

        internal static string OnPropertyChanged(IMethodSymbol invoker, IPropertySymbol property, bool usesUnderscoreNames)
        {
            if (invoker.IsCallerMemberName())
            {
                return usesUnderscoreNames
                    ? $"{invoker.Name}();"
                    : $"this.{invoker.Name}();";
            }

            return OnOtherPropertyChanged(invoker, property.Name, usesUnderscoreNames);
        }

        internal static string OnOtherPropertyChanged(IMethodSymbol invoker, string propertyName, bool usesUnderscoreNames)
        {
            if (invoker == null)
            {
                return usesUnderscoreNames
                    ? $"PropertyChanged?.Invoke(new System.ComponentModel.PropertyChangedEventArgs({propertyName}));"
                    : $"this.PropertyChanged?.Invoke(new System.ComponentModel.PropertyChangedEventArgs(nameof(this.{propertyName})));";
            }

            if (invoker.Parameters.TryGetSingle(out var parameter))
            {
                if (parameter.Type == KnownSymbol.String)
                {
                    return usesUnderscoreNames
                        ? $"{invoker.Name}(nameof({propertyName}));"
                        : $"this.{invoker.Name}(nameof(this.{propertyName}));";
                }

                if (parameter.Type == KnownSymbol.PropertyChangedEventArgs)
                {
                    return usesUnderscoreNames
                        ? $"{invoker.Name}(new System.ComponentModel.PropertyChangedEventArgs({propertyName}));"
                        : $"this.{invoker.Name}(new System.ComponentModel.PropertyChangedEventArgs(nameof(this.{propertyName})));";
                }
            }

            return "GeneratedSyntaxErrorBugInPropertyChangedAnalyzersCodeFixes";
        }
    }
}
