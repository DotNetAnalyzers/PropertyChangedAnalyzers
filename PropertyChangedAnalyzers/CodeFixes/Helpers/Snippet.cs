namespace PropertyChangedAnalyzers
{
    using System;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;

    [Obsolete("Remove this.")]
    internal static class Snippet
    {
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
