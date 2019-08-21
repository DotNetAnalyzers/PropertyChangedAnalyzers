namespace PropertyChangedAnalyzers
{
    using System;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;

    [Obsolete("Remove this.")]
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
                if (Gu.Roslyn.AnalyzerExtensions.Equality.HasEqualityOperator(type))
                {
                    return $"{x} == {y}";
                }

                if (type == KnownSymbol.NullableOfT)
                {
                    return Gu.Roslyn.AnalyzerExtensions.Equality.HasEqualityOperator(((INamedTypeSymbol)type).TypeArguments[0])
                        ? $"{x} == {y}"
                        : $"System.Nullable.Equals({x}, {y})";
                }

                if (type.GetMembers(nameof(Equals))
                        .OfType<IMethodSymbol>()
                        .TrySingle(
                            m => m.Parameters.Length == 1 &&
                                 ReferenceEquals(type, m.Parameters[0].Type), out _))
                {
                    return $"{x}.Equals({y})";
                }

                return $"System.Collections.Generic.EqualityComparer<{type.ToDisplayString()}>.Default.Equals({x}, {y})";
            }

            if (Descriptors.INPC006UseReferenceEqualsForReferenceTypes.IsSuppressed(semanticModel))
            {
                return $"Equals({x}, {y})";
            }

            return $"ReferenceEquals({x}, {y})";
        }

        internal static string OnPropertyChanged(IMethodSymbol invoker, string propertyName, bool usesUnderscoreNames)
        {
            if (invoker != null &&
                invoker.Parameters.TrySingle(out var parameter) &&
                parameter.IsCallerMemberName())
            {
                return usesUnderscoreNames
                    ? $"{invoker.Name}();"
                    : $"this.{invoker.Name}();";
            }

            return OnOtherPropertyChanged(invoker, propertyName, usesUnderscoreNames);
        }

        internal static string OnOtherPropertyChanged(IMethodSymbol invoker, string propertyName, bool usesUnderscoreNames)
        {
            if (invoker == null)
            {
                return usesUnderscoreNames
                    ? $"PropertyChanged?.Invoke(new System.ComponentModel.PropertyChangedEventArgs(nameof({propertyName})));"
                    : $"this.PropertyChanged?.Invoke(new System.ComponentModel.PropertyChangedEventArgs(nameof(this.{propertyName})));";
            }

            if (invoker.Parameters.TrySingle(out var parameter))
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
                        ? $"{invoker.Name}(new System.ComponentModel.PropertyChangedEventArgs(nameof({propertyName})));"
                        : $"this.{invoker.Name}(new System.ComponentModel.PropertyChangedEventArgs(nameof(this.{propertyName})));";
                }
            }

            return "GeneratedSyntaxErrorBugInPropertyChangedAnalyzersCodeFixes";
        }
    }
}
